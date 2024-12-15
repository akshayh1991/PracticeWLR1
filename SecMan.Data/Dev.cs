using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data
{
    public class Dev
    {
        internal Dev(SQLCipher.Dev sqlCipherDev)
        {
            Id = sqlCipherDev.Id;
            Name = sqlCipherDev.Name;
            DevDef = new(null, sqlCipherDev.DevDef);
            Zone = sqlCipherDev.Zone == null ? null : new(sqlCipherDev.Zone, false);
            Vers = sqlCipherDev.Vers;
            Legacy = sqlCipherDev.Legacy;
            SysPolVer = sqlCipherDev.SysPolVer;
            DevPolVer = sqlCipherDev.DevPolVer;
            DevPermVer = sqlCipherDev.DevPermVer;
            UserVer = sqlCipherDev.UserVer;
            RoleVer = sqlCipherDev.RoleVer;
            ConnRate = sqlCipherDev.ConnRate;
            ConnState = sqlCipherDev.ConnState;
            LastConnDate = sqlCipherDev.LastConnDate; 
        }
        public ulong Id { get; set; }
        public string? Name { get; set; }
 
        public DevDef DevDef { get; set; }
        public Zone? Zone { get; set; }
        public string? Vers { get; set; }
        public bool Legacy { get; set; }
        public ulong SysPolVer { get; set; }
        public ulong DevPolVer { get; set; }
        public ulong DevPermVer { get; set; }
        public ulong UserVer { get; set; }
        public ulong RoleVer { get; set; }
        public int ConnRate { get; set; }
        public int ConnState { get; set; }
        public DateTime LastConnDate { get; set; }

        public bool AddZone(ulong zoneId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                if ((db.Zones != null) && (db.Devs != null))
                {
                    SQLCipher.Zone sqlCipherZone = db.Zones
                        .Where(o => o.Id == zoneId)
                        .FirstOrDefault();

                    SQLCipher.Dev sqlCipherDev = db.Devs
                        .Include(o => o.Zone)
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                    if ((sqlCipherDev != null) && (sqlCipherZone != null))
                    {
                        Zone zone = new(sqlCipherZone, false);
                        zone.AddDev(sqlCipherDev.Id, false);
                        sqlCipherDev.Zone = sqlCipherZone;
                        db.SaveChanges();
                        Zone = zone;
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
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }
        public bool RemZone()
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                if (db.Devs != null)
                {
                    SQLCipher.Dev sqlCipherDev = db.Devs
                        .Include(o => o.Zone)
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                    if ((sqlCipherDev != null) && (sqlCipherDev.Zone != null))
                    {
                        sqlCipherDev.Zone = null;
                        db.SaveChanges();
                        Zone = null;
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
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }
    }
}
