using Microsoft.EntityFrameworkCore;
using SecMan.Data.Init;
using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Data.SecManDb;

namespace SecMan.Data
{
    public class Zone
    {
        internal Zone(SQLCipher.Zone zone, bool includeRoles)
        {
            Id = zone.Id;
            Name = zone.Name;
            zone.Roles.ForEach(o => Roles.Add(new(o, includeRoles)));
        }
        public ulong Id { get; set; }
        public string? Name { get; set; }

        public List<Dev> Devs { get; set; } = [];
        public List<RoleData> Roles { get; set; } = [];

        public bool AddRole(ulong roleId)
        {
            return AddRole(roleId, true);
        }
        internal bool AddRole(ulong roleId, bool lockDb)
        {
            bool ok = false;
            if (lockDb)
            {
                SecManDb.dbLock.EnterWriteLock();
            }
            try
            {
                using SQLCipher.Db db = new();
                if ((Roles != null) && (db.Zones != null) && (db.Roles != null))
                {
                    if (!Roles.Any(o => o.Id == roleId))
                    {

                        SQLCipher.Zone sqlCipherZone = db.Zones
                            .Include(o => o.Roles)
                            .Where(o => o.Id == Id)
                            .FirstOrDefault();

                        SQLCipher.Role sqlCipherRole = db.Roles
                            .Where(o => o.Id == roleId)
                            .FirstOrDefault();

                        if ((sqlCipherZone != null) && (sqlCipherRole != null))
                        {
                            sqlCipherZone.Roles.Add(sqlCipherRole);
                            Roles.Add(new(sqlCipherRole, false));
                            sqlCipherRole = db.Roles
                                .Where(o => o.Id == roleId)
                                .FirstOrDefault();

                            // Add the Role permissions for each DevDef
                            List<SQLCipher.Dev> sqlCipherDevs = db.Devs
                               .Where(o => o.Zone.Id == Id)
                               .Include(x => x.DevDef)
                               .Include(x => x.DevDef.DevPolDefs)
                               .Include(x => x.DevDef.DevPermDefs)
                               .ToList();
                            List<SQLCipher.DevDef> sqlCipherDevDefs = sqlCipherDevs.Select(o => o.DevDef).Distinct().ToList();
                            foreach (SQLCipher.DevDef sqlCipherDevDef in sqlCipherDevDefs)
                            {
                                foreach (SQLCipher.DevPermDef devPermDef in sqlCipherDevDef.DevPermDefs)
                                {
                                    SQLCipher.DevPermVal? devPermVal = db.DevPermVals
                                        .Where(o => o.Zone.Id == Id && o.DevDef.Id == sqlCipherDevDef.Id && o.DevPermDef.Id == devPermDef.Id)
                                        .FirstOrDefault();
                                    if (devPermVal == null)
                                    {
                                        devPermVal = new()
                                        {
                                            Zone = sqlCipherZone,
                                            DevDef = sqlCipherDevDef,
                                            Role = sqlCipherRole,
                                            DevPermDef = devPermDef,
                                            Val = false
                                        };
                                        db.Add(devPermVal);
                                    }
                                }
                            }
                            db.SaveChanges();
                            ok = true;
                        }
                    }
                }
            }
            catch
            {
                ok = false;
            }
            finally
            {
                if (lockDb)
                {
                    SecManDb.dbLock.ExitWriteLock();
                }
            }
            return ok;
        }
        public bool RemRole(ulong roleId)
        {
            return RemRole(roleId, true);
        }

        internal bool RemRole(ulong roleId, bool lockDb)
        {
            bool ok = false;
            if (lockDb)
            {
                SecManDb.dbLock.EnterWriteLock();
            }
            try
            {
                using SQLCipher.Db db = new();
                if ((Roles != null) && (db.Zones != null) && (db.Roles != null))
                {
                    if (Roles.Any(o => o.Id == roleId))
                    {
                        SQLCipher.Zone sqlCipherZone = db.Zones
                        .Include(o => o.Roles)
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                        SQLCipher.Role sqlCipherRole = db.Roles
                        .Where(o => o.Id == roleId)
                        .FirstOrDefault();


                        if ((sqlCipherZone != null) && (sqlCipherRole != null))
                        {
                            sqlCipherZone.Roles.Remove(sqlCipherRole);
                            db.SaveChanges();
                            Roles.RemoveAll(o => o.Id == roleId);
                            ok = true;
                        }
                    }
                }
            }
            catch
            {
                ok = false;
            }
            finally
            {
                if (lockDb)
                {
                    SecManDb.dbLock.ExitWriteLock();
                }
            }
            return ok;
        }

        private void UpdateDevs(SQLCipher.Db db)
        {
            Devs.Clear();
            db.Devs
                .Include(o => o.DevDef)
                .Where(o => o.Zone != null && o.Zone.Id == Id)
                .ToList()
                .ForEach(o => Devs.Add(new(o)));
        }

        public bool AddDev(ulong devId)
        {
            return AddDev(devId, true);
        }

        internal bool AddDev(ulong devId, bool lockDb)
        {
            bool ok = false;
            if (lockDb)
            {
                SecManDb.dbLock.EnterWriteLock();
            }
            try
            {
                using SQLCipher.Db db = new();
                if ((db.Zones != null) && (db.Devs != null))
                {
                    SQLCipher.Zone sqlCipherZone = db.Zones
                        .Include(o => o.Roles)
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                    SQLCipher.Dev sqlCipherDev = db.Devs
                        .Include(o => o.DevDef)
                        .Include(o => o.DevDef.DevPermDefs)
                        .Include(o => o.DevDef.DevPolDefs)
                        .Where(o => o.Id == devId)
                        .FirstOrDefault();

                    if ((sqlCipherDev.Zone == null) || (sqlCipherDev.Zone != sqlCipherZone))
                    {
                        // The device is not allocated or allocated to another zone
                        sqlCipherDev.Zone = sqlCipherZone;

                        // Get the device's DevDef
                        SQLCipher.DevDef? sqlCipherDevDef = db.DevDefs
                            .Where(o => o.Id == sqlCipherDev.DevDef.Id)
                            .FirstOrDefault();

                        // Add DevDef Policy Values
                        foreach (SQLCipher.DevPolDef devPolDef in sqlCipherDevDef.DevPolDefs)
                        {
                            SQLCipher.DevPolVal? devPolVal = db.DevPolVals
                                .Include(o => o.Zone)
                                .Include(o => o.DevDef)
                                .Include(o => o.DevPolDef)
                                .Where(o => o.Zone.Id == sqlCipherZone.Id && o.DevDef.Id == sqlCipherDevDef.Id && o.DevPolDef.Id == devPolDef.Id)
                                .FirstOrDefault();
                            if (devPolVal == null)
                            {
                                devPolVal = new()
                                {
                                    Zone = sqlCipherZone,
                                    DevDef = sqlCipherDev.DevDef,
                                    DevPolDef = devPolDef,
                                    Val = devPolDef.ValDflt
                                };
                                db.Add(devPolVal);
                            }
                        }

                        // Add signature values
                        foreach (SQLCipher.DevPermDef devPermDef in sqlCipherDevDef.DevPermDefs)
                        {
                            SQLCipher.DevSigVal? devSigVal = db.DevSigVals
                                .Include(o => o.Zone)
                                .Include(o => o.DevDef)
                                .Include(o => o.DevPermDef)
                                .Where(o => o.Zone.Id == sqlCipherZone.Id && o.DevDef.Id == sqlCipherDevDef.Id && o.DevPermDef.Id == devPermDef.Id)
                                .FirstOrDefault();
                            if (devSigVal == null)
                            {
                                {
                                    devSigVal = new()
                                    {
                                        Zone = sqlCipherZone,
                                        DevDef = sqlCipherDev.DevDef,
                                        DevPermDef = devPermDef,
                                        Sign = false,
                                        Auth = false,
                                        Note = false
                                    };
                                    db.Add(devSigVal);
                                }
                            }
                        }

                        // Add role permissions
                        if (sqlCipherZone.Roles != null)
                        {
                            foreach (SQLCipher.Role sqlCipherRole in sqlCipherZone.Roles)
                            {
                                foreach (SQLCipher.DevPermDef devPermDef in sqlCipherDevDef.DevPermDefs)
                                {
                                    SQLCipher.DevPermVal? devPermVal = db.DevPermVals
                                        .Where(o => o.Zone.Id == Id && o.DevDef.Id == sqlCipherDevDef.Id && o.DevPermDef.Id == devPermDef.Id)
                                        .FirstOrDefault();
                                    if (devPermVal == null)
                                    {
                                        devPermVal = new()
                                        {
                                            Zone = sqlCipherZone,
                                            DevDef = sqlCipherDevDef,
                                            Role = sqlCipherRole,
                                            DevPermDef = devPermDef,
                                            Val = false
                                        };
                                        db.Add(devPermVal);
                                    }
                                }
                            }
                        }
                        db.SaveChanges();
                        UpdateDevs(db);
                        ok = true;
                    }
                }
            }
            catch
            {
                ok = false;
            }
            finally
            {
                if (lockDb)
                {
                    SecManDb.dbLock.ExitWriteLock();
                }
            }
            return ok;
        }

        public bool RemDev(ulong devId)
        {
            return RemDev(devId, true);
        }

        internal bool RemDev(ulong devId, bool lockDb)
        {
            bool ok = false;
            if (lockDb)
            {
                SecManDb.dbLock.EnterWriteLock();
            }
            try
            {
                using SQLCipher.Db db = new();
                if ((db.Zones != null) && (db.Devs != null))
                {
                    SQLCipher.Zone sqlCipherZone = db.Zones
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                    SQLCipher.Dev sqlCipherDev = db.Devs
                        .Where(o => o.Id == devId)
                        .FirstOrDefault();

                    if ((sqlCipherDev.Zone != null) || (sqlCipherDev.Zone == sqlCipherZone))
                    {
                        // The device is allocated to this zone
                        sqlCipherDev.Zone = null;
                        db.SaveChanges();
                        UpdateDevs(db);
                        ok = true;
                    }
                }
            }
            catch
            {
                ok = false;
            }
            finally
            {
                if (lockDb)
                {
                    SecManDb.dbLock.ExitWriteLock();
                }
            }
            return ok;
        }

        public List<DevDef> GetDevDefs(string lang)
        {
            List<DevDef> devDefs = new();
            SecManDb.dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();
                db.Devs
                    .Include(o => o.DevDef)
                    .Include(o => o.DevDef.Langs)
                    .Include(o => o.DevDef.DevPolDefs)
                    .ThenInclude(p => p.Langs)
                    .Include(o => o.DevDef.DevPermDefs)
                    .ThenInclude(p => p.Langs)
                    .Where(o => o.Zone != null && o.Zone.Id == Id)
                    .Select(o => o.DevDef)
                    .Distinct()
                    .ToList()
                    .ForEach(o => devDefs.Add(new(lang, o)));
            }
            catch
            {
                devDefs = new();
            }
            finally
            {
                SecManDb.dbLock.ExitReadLock();
            }
            return devDefs;
        }

        public List<DevPolVal> GetDevDefPolVals(ulong devDefId)
        {
            List<DevPolVal> devPolVals = new();
            SecManDb.dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();
                db.DevPolVals
                    .Include(o => o.Zone)
                    .Include(o => o.DevDef)
                    .Include(o => o.DevPolDef)
                    .Where(o => o.Zone.Id == Id && o.DevDef.Id == devDefId)
                    .ToList()
                    .ForEach(o => devPolVals.Add(new(o)));
            }
            catch
            {
                devPolVals = new();
            }
            finally
            {
                SecManDb.dbLock.ExitReadLock();
            }
            return devPolVals;
        }

        public List<DevSigVal> GetDevDefSigVals(ulong devDefId)
        {
            List<DevSigVal> devSigVals = new();
            SecManDb.dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();
                db.DevSigVals
                    .Include(o => o.Zone)
                    .Include(o => o.DevDef)
                    .Include(o => o.DevPermDef)
                    .Where(o => o.Zone.Id == Id && o.DevDef.Id == devDefId)
                    .ToList()
                    .ForEach(o => devSigVals.Add(new(o)));
            }
            catch
            {
                devSigVals = new();
            }
            finally
            {
                SecManDb.dbLock.ExitReadLock();
            }
            return devSigVals;
        }

        public List<DevPermVal> GetDevDefRolePerms(ulong devDefId, ulong roleId)
        {
            List<DevPermVal> devPermVals = new();
            SecManDb.dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();
                db.DevPermVals
                    .Include(o => o.Zone)
                    .Include(o => o.DevDef)
                    .Include(o => o.Role)
                    .Include(o => o.DevPermDef)
                   .Where(o => o.Zone.Id == Id && o.DevDef.Id == devDefId && o.Role.Id == roleId)
                    .ToList()
                    .ForEach(o => devPermVals.Add(new(o)));
            }
            catch
            {
                devPermVals = new();
            }
            finally
            {
                SecManDb.dbLock.ExitReadLock();
            }
            return devPermVals;
        }

    }
}
