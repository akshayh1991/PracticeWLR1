using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecMan.Data.Exceptions;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.DAL;
using SecMan.Model;

namespace SecMan.Data
{
    public class RoleData 
    {
        SecManDb db = new();
        #region Jon's Code
        internal RoleData(SQLCipher.Role role, bool includeUsers)
        {
            if (role != null)
            {
                Id = role.Id;
                _name = role.Name;

                // Get the users
                if (includeUsers)
                {
                    try
                    {
                        using SQLCipher.Db db = new();

                        // Get the Users allocated to this role
                        if (db.Users != null)
                        {
                            db.Users
                                .Include(o => o.Roles)
                                .Where(x => x.Roles.Contains(role))
                                .ToList()
                                .ForEach(x => Users.Add(new(x, false)));
                        }
                        if (db.Zones != null)
                        {
                            db.Zones
                                .Include(o => o.Roles)
                                .Where(x => x.Roles.Contains(role))
                                .ToList()
                                .ForEach(x => Zones.Add(new(x, false)));
                        }
                    }
                    catch { }
                }
                else
                {
                    Users = null;
                }
            }
        }

        public ulong Id { get; set; }
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
        }
        public SecManDb.ReturnCode SetName(string name)
        {
            SecManDb.ReturnCode returnCode = SecManDb.ReturnCode.Unknown;
            name = name.Trim();
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                if (db.Roles != null)
                {
                    // Check name is unique
                    SQLCipher.Role? sqlCipherRole = db.Roles
                        .Where(x => x.Name.ToUpper() == name.ToUpper())
                        .FirstOrDefault();
                    if ((sqlCipherRole != null) && (sqlCipherRole.Id != Id))
                    {
                        returnCode = SecManDb.ReturnCode.NameNotUnique;

                    }
                    else
                    {
                        sqlCipherRole = db.Roles
                            .Where(x => x.Id == Id)
                            .FirstOrDefault();

                        if (sqlCipherRole != null)
                        {
                            sqlCipherRole.Name = name;
                            db.SaveChanges();
                            returnCode = SecManDb.ReturnCode.Ok;
                        }
                    }
                }
            }
            catch
            {
                returnCode = SecManDb.ReturnCode.Unknown;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return returnCode;
        }
        public List<User>? Users { get; set; } = [];
        public List<Zone>? Zones { get; set; } = [];
        public bool AddUser(ulong userId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                // Get the User
                SQLCipher.User? sqlCipherUser = null;
                if (db.Users != null)
                {
                    sqlCipherUser = db.Users
                      .Where(x => x.Id == userId)
                      .FirstOrDefault();
                }

                // Get the Role
                if ((sqlCipherUser != null) && (db.Roles != null))
                {
                    SQLCipher.Role? sqlCipherRole = db.Roles
                       .Include(o => o.Users)
                       .Where(x => x.Id == Id)
                      .FirstOrDefault();

                    if (sqlCipherRole != null)
                    {
                        // Add the user to the role
                        if (!sqlCipherRole.Users.Contains(sqlCipherUser))
                        {
                            sqlCipherRole.Users.Add(sqlCipherUser);
                            db.SaveChanges();
                            Users.Add(new(sqlCipherUser, false));
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
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }



        public bool RemUser(ulong userId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                User user = Users.Where(o => o.Id == userId).FirstOrDefault();
                if (user != null)
                {
                    {

                        // Get the User
                        SQLCipher.User? sqlCipherUser = null;
                        if (db.Users != null)
                        {
                            sqlCipherUser = db.Users
                               .Where(x => x.Id == userId)
                              .FirstOrDefault();
                        }

                        // Get the Role
                        if ((sqlCipherUser != null) && (db.Roles != null))
                        {
                            SQLCipher.Role? sqlCipherRole = db.Roles
                               .Include(o => o.Users)
                               .Where(x => x.Id == Id)
                              .FirstOrDefault();

                            if (sqlCipherRole != null)
                            {
                                sqlCipherRole.Users.Remove(sqlCipherUser);
                                db.SaveChanges();
                                Users.Remove(user);
                                ok = true;
                            }
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
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }

        public bool AddZone(ulong zoneId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                if ((db.Zones != null) && (db.Roles != null))
                {
                    SQLCipher.Zone sqlCipherZone = db.Zones
                        .Where(o => o.Id == zoneId)
                        .FirstOrDefault();

                    SQLCipher.Role sqlCipherRole = db.Roles
                        .Include(o => o.Zones)
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                    if ((sqlCipherRole != null) && (!sqlCipherRole.Zones.Contains(sqlCipherZone)))
                    {
                        Zone zone = new(sqlCipherZone, false);
                        zone.AddRole(sqlCipherRole.Id, false);
                        Zones.Add(zone);
                        db.SaveChanges();
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
        public bool RemZone(ulong zoneId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                if ((db.Zones != null) && (db.Roles != null))
                {
                    SQLCipher.Zone sqlCipherZone = db.Zones
                        .Where(o => o.Id == zoneId)
                        .FirstOrDefault();

                    SQLCipher.Role sqlCipherRole = db.Roles
                        .Include(o => o.Zones)
                        .Where(o => o.Id == Id)
                        .FirstOrDefault();

                    if ((sqlCipherRole != null) && (sqlCipherRole.Zones.Contains(sqlCipherZone)))
                    {
                        sqlCipherZone.Roles.Add(sqlCipherRole);
                        db.SaveChanges();
                        Zone zone = Zones.Where(o => o.Id == zoneId).FirstOrDefault();
                        Zones.Remove(zone);
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
        #endregion
       
    }
}
