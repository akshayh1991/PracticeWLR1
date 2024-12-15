using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using SecMan.Data.Init;
using SecMan.Data.SQLCipher;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SecMan.Data
{
    public class SecManDb
    {
        #region Initialise database

        internal static ReaderWriterLockSlim dbLock = new();

        public enum ValType
        {
            None = 0,   // undefined
            Str = 1,    // string
            Int = 2,    // integer
            Bool = 3,   // boolean
            IP = 4,     // ip address
            Email = 5   // email address
        }

        public enum ReturnCode
        {
            Ok = 0,
            Unknown = -1,
            NonExistantObject = -2,
            InvalidFormat = -3,
            InvalidUser = -4,
            InvalidDevice = -5,
            NameNotUnique = -6
        }

        public SecManDb()
        {
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                // Loop round the initialization files
                string defConfFilePath = string.Empty;
                InitFileType defConfinitFileType = null;
                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(db.DbPath));
                foreach (var fi in di.GetFiles("*.json"))
                {
                    bool loaded = false;
                    InitFileType initFileType = GetInitFileType(fi.FullName);
                    if (!InitFileLoaded(db, initFileType))
                    {
                        switch (initFileType.Type)
                        {
                            case "SysFeat":
                                loaded = LoadSysFeatInitFile(db, fi.FullName);
                                break;
                            case "DevDef":
                                loaded = LoadDevDefInitFile(db, fi.FullName);
                                break;
                            case "DevList":
                                loaded = LoadApplicationLauncherInitFile(db, fi.FullName);
                                break;
                            case "RSAKeys":
                                loaded = LoadRsaKeys(db, fi.FullName);
                                break;
                            case "DefConf":
                                // The default configuration must be loaded after all other initialisation
                                defConfFilePath = fi.FullName;
                                defConfinitFileType = new InitFileType()
                                {
                                    Type = initFileType.Type,
                                    Name = initFileType.Name,
                                    Vers = initFileType.Vers
                                };
                                break;
                            default:
                                break;
                        }
                        if (loaded)
                        {
                            UpdateInitFileLoaded(db, initFileType);
                        }
                    }
                }

                // Load the default configuration
                if (!string.IsNullOrEmpty(defConfFilePath))
                {
                    if (LoadDefConfInitFile(db, defConfFilePath))
                    {
                        UpdateInitFileLoaded(db, defConfinitFileType);
                    }
                }
            }
            catch
            {

            }
            finally
            {
                dbLock.ExitWriteLock();
            }
        }

        internal InitFileType GetInitFileType(string filePath)
        {
            InitFileType initFileType = new();
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    if (json != null)
                    {
                        initFileType = JsonSerializer.Deserialize<InitFileType>(json);
                        if (initFileType == null)
                        {
                            initFileType = new();
                        }
                    }
                    else
                    {
                        initFileType = new();
                    }
                }
            }
            catch (Exception ex)
            {
                initFileType = new();
            }
            return initFileType;
        }

        internal bool InitFileLoaded(SQLCipher.Db db, InitFileType initFileType)
        {
            bool loaded = false;
            InitFileType initFileLoaded = db.InitFileTypes.Where(x => x.Type == initFileType.Type && x.Name == initFileType.Name).FirstOrDefault();
            if ((initFileLoaded != null) && (initFileLoaded.Vers >= initFileType.Vers))
            {
                loaded = true;
            }
            return loaded;
        }

        internal void UpdateInitFileLoaded(SQLCipher.Db db, InitFileType initFileType)
        {
            InitFileType initFileLoaded = db.InitFileTypes.Where(x => x.Type == initFileType.Type && x.Name == initFileType.Name).FirstOrDefault();
            if (initFileLoaded == null)
            {
                initFileLoaded = new()
                {
                    Type = initFileType.Type,
                    Name = initFileType.Name,
                    Vers = initFileType.Vers
                };
                db.InitFileTypes.Add(initFileLoaded);
            }
            else
            {
                initFileLoaded.Vers = initFileType.Vers;
            }
            db.SaveChanges();
        }


        internal bool LoadSysFeatInitFile(SQLCipher.Db db, string filePath)
        {
            bool ok = false;
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    SQLCipher.SysFeat? sysFeat = JsonSerializer.Deserialize<SQLCipher.SysFeat>(json);
                    if ((sysFeat != null) && (sysFeat.Name != null))
                    {
                        // Does the System Feature already exist
                        if (db.SysFeats.Where(x => x.Name == sysFeat.Name).FirstOrDefault() == null)
                        {
                            // The Device Type does not exist so add it
                            db.Add(sysFeat);
                            db.SaveChanges();
                        }
                        ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return ok;
        }

        internal bool LoadDevDefInitFile(SQLCipher.Db db, string filePath)
        {
            bool ok = false;
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    SQLCipher.DevDef? devDefInit = JsonSerializer.Deserialize<SQLCipher.DevDef>(json);
                    if ((devDefInit != null) && (devDefInit.Name != null))
                    {
                        // Does the Dev Type Definition already exist
                        if (db.DevDefs.Where(x => x.Name == devDefInit.Name).FirstOrDefault() == null)
                        {
                            // The Device Type does not exist so add it
                            db.Add(devDefInit);
                            db.SaveChanges();
                        }
                        ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return ok;
        }
        internal bool LoadDefConfInitFile(SQLCipher.Db db, string filePath)
        {
            bool ok = false;
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    DefConfInit defConfInit = JsonSerializer.Deserialize<DefConfInit>(json);
                    if (defConfInit != null)
                    {
                        if (defConfInit.Zones != null)
                        {
                            foreach (string zoneName in defConfInit.Zones)
                            {
                                // Check Zone exists
                                SQLCipher.Zone? zone = db.Zones.Where(x => x.Name == zoneName).FirstOrDefault();

                                if (zone == null)
                                {
                                    zone = new()
                                    {
                                        Name = zoneName,
                                    };
                                    db.Add(zone);
                                    db.SaveChanges();
                                }
                            }
                        }

                        if (defConfInit.Roles != null)
                        {
                            foreach (RoleInit roleInit in defConfInit.Roles)
                            {
                                // Validate Zones
                                List<SQLCipher.Zone> zones = new();
                                foreach (string zoneName in roleInit.Zones)
                                {
                                    // Check zone exists
                                    SQLCipher.Zone? zone = db.Zones.Where(x => x.Name == zoneName).FirstOrDefault();
                                    if (zone != null)
                                    {
                                        zones.Add(zone);
                                    }
                                }

                                // Create the Role
                                SQLCipher.Role role = db.Roles.Where(x => x.Name == roleInit.Name).FirstOrDefault();
                                if (role == null)
                                {
                                    role = new();
                                    role.Name = roleInit.Name;
                                    db.Add(role);
                                }
                                role.Zones = zones;
                                db.SaveChanges();
                            }
                        }

                        if (defConfInit.Users != null)
                        {
                            foreach (UserInit userInit in defConfInit.Users)
                            {
                                // Does the User already exist
                                if (db.Users.Where(x => x.UserName == userInit.UserName).FirstOrDefault() == null)
                                {
                                    // The User does not exist so add it
                                    SQLCipher.User user = new()
                                    {
                                        Domain = userInit.Domain,
                                        UserName = userInit.UserName,
                                        Password = userInit.Password
                                    };


                                    // Validate the Roles
                                    List<SQLCipher.Role> roles = new();
                                    if (userInit.Roles != null)
                                    {
                                        foreach (string roleName in userInit.Roles)
                                        {
                                            // Check role exists
                                            SQLCipher.Role? role = db.Roles.Where(x => x.Name == roleName).FirstOrDefault();
                                            if (role != null)
                                            {
                                                roles.Add(role);
                                            }
                                        }
                                    }

                                    user.Roles = roles;

                                    db.Add(user);
                                    db.SaveChanges();
                                }
                            }
                        }

                        if (defConfInit.Devs != null)
                        {
                            foreach (DevInit devInit in defConfInit.Devs)
                            {
                                // Check DevDef exists
                                SQLCipher.Dev dev = db.Devs.Where(x => x.Name == devInit.Name).FirstOrDefault();
                                if (dev == null)
                                {
                                    SQLCipher.DevDef? devDef = db.DevDefs.Where(x => x.Name == devInit.DevDef).FirstOrDefault();
                                    SQLCipher.Zone? zone = db.Zones.Where(x => x.Name == devInit.Zone).FirstOrDefault();
                                    if (devDef != null)
                                    {
                                        dev = new();
                                        dev.Name = devInit.Name;
                                        dev.DevDef = devDef;
                                        dev.Zone = zone;
                                        db.Add(dev);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        // Add the Device Policy and Permission Values
                        foreach (SQLCipher.Zone zone in db.Zones)
                        {
                            // Get the unique device types
                            List<SQLCipher.Dev> devs = db.Devs
                                .Where(o => o.Zone == zone)
                                .Include(x => x.DevDef)
                                .Include(x => x.DevDef.DevPolDefs)
                                .Include(x => x.DevDef.DevPermDefs)
                                .ToList();
                            List<SQLCipher.DevDef> zoneDevDefs = devs.Select(o => o.DevDef).Distinct().ToList();

                            foreach (SQLCipher.DevDef devDef in zoneDevDefs)
                            {
                                // Add Device Policies
                                if (devDef.DevPolDefs != null)
                                {
                                    DevPolInit devPolInit = defConfInit.DevPols.Where(o => o.Zone == zone.Name && o.DevDef == devDef.Name).FirstOrDefault();

                                    foreach (SQLCipher.DevPolDef devPolDef in devDef.DevPolDefs)
                                    {
                                        PolInit polInit = null;
                                        if (devPolInit != null)
                                        {
                                            polInit = devPolInit.Pols.Where(o => o.Name == devPolDef.Name).FirstOrDefault();
                                        }

                                        SQLCipher.DevPolVal devPolVal = new()
                                        {
                                            Zone = zone,
                                            DevDef = devDef,
                                            DevPolDef = devPolDef,
                                            Val = polInit == null ? string.Empty : polInit.Val
                                        };
                                        db.Add(devPolVal);
                                    }
                                }

                                // Check device has permissions
                                if (devDef.DevPermDefs != null)
                                {
                                    DevSigInit devSigInit = defConfInit.DevSigs.Where(o => o.Zone == zone.Name && o.DevDef == devDef.Name).FirstOrDefault();

                                    // Add Device Signatures
                                    foreach (SQLCipher.DevPermDef devPermDef in devDef.DevPermDefs)
                                    {
                                        SigInit sigInit = null;
                                        if (devSigInit != null)
                                        {
                                            sigInit = devSigInit.Signatures.Where(o => o.Perm == devPermDef.Name).FirstOrDefault();
                                        }

                                        SQLCipher.DevSigVal devSigVal = new()
                                        {
                                            Zone = zone,
                                            DevDef = devDef,
                                            DevPermDef = devPermDef,
                                            Sign = sigInit == null ? false : sigInit.Sign,
                                            Auth = sigInit == null ? false : sigInit.Auth,
                                            Note = sigInit == null ? false : sigInit.Note
                                        };
                                        db.Add(devSigVal);
                                    }

                                    // Add Device Role Permissions
                                    foreach (SQLCipher.Role role in zone.Roles)
                                    {
                                        DevPermInit devPermInit = defConfInit.DevPerms.Where(o => o.Zone == zone.Name && o.DevDef == devDef.Name && o.Role == role.Name).FirstOrDefault();
                                        if (devPermInit != null)
                                        {
                                            foreach (SQLCipher.DevPermDef devPermDef in devDef.DevPermDefs)
                                            {
                                                SQLCipher.DevPermVal devPermVal = new()
                                                {
                                                    Zone = zone,
                                                    DevDef = devDef,
                                                    Role = role,
                                                    DevPermDef = devPermDef,
                                                    Val = devPermInit.Perms.Contains(devPermDef.Name) ? true : false
                                                };
                                                db.Add(devPermVal);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            db.SaveChanges();

                        }
                    }
                }
                ok = true;
            }
            catch (Exception ex)
            {
            }
            return ok;
        }

        #endregion
        #region SuperUser
        public SuperUser? GetSuperUser()
        {
            SuperUser? superUser = null;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.SuperUsers != null))
                {
                    SQLCipher.SuperUser? sqlCipherSuperUser = db.SuperUsers.FirstOrDefault();

                    if (sqlCipherSuperUser == null)
                    {
                        sqlCipherSuperUser = new();
                        db.Add(sqlCipherSuperUser);
                        db.SaveChanges();
                        sqlCipherSuperUser = db.SuperUsers.FirstOrDefault();
                    }

                    if (sqlCipherSuperUser != null)
                    {
                        superUser = new(sqlCipherSuperUser);
                    }
                }
            }
            catch
            {
                superUser = null;
            }
            finally
            {
                dbLock.ExitWriteLock();
            }
            return superUser;

        }
        #endregion

        #region Users
        public List<User> GetUsers()
        {
            // Get the Roles
            List<User> users = new();
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Users != null))
                {
                    db.Users
                        .Include(x => x.Roles)
                        .ToList()
                        .ForEach(o => users.Add(new(o, true)));
                }
            }
            catch
            {
                users = new();
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return users;
        }


        public User? GetUser(ulong userId)
        {
            User? user = null;
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Users != null))
                {
                    SQLCipher.User? sqlCipherUser = db.Users
                        .Where(o => o.Id == userId)
                        .FirstOrDefault();
                    if (sqlCipherUser != null)
                    {
                        user = new(sqlCipherUser, true);
                    }
                }
            }
            catch
            {
                user = null;
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return user;
        }

        public User? GetUser(string userName)
        {
            return GetUser(string.Empty, userName);
        }

        public User? GetUser(string domain, string userName)
        {
            User? user = null;
            domain = domain.ToUpper().Trim();
            userName = userName.ToUpper().Trim();
            if (userName != null)
            {
                dbLock.EnterReadLock();
                try
                {
                    using SQLCipher.Db db = new();

                    if ((db != null) && (db.Users != null))
                    {
                        SQLCipher.User? sqlCipherUser = null;
                        if (string.IsNullOrEmpty(domain))
                        {
                            sqlCipherUser = db.Users
                                .Where(o => o.UserName.ToUpper() == userName)
                                .FirstOrDefault();
                        }
                        else
                        {
                            sqlCipherUser = db.Users
                                .Where(o => o.Domain.ToUpper() == domain && o.UserName.ToUpper() == userName)
                                .FirstOrDefault();

                        }
                        if (sqlCipherUser != null)
                        {
                            user = new(sqlCipherUser, true);
                        }
                    }
                }
                catch
                {
                    user = null;
                }
                finally
                {
                    dbLock.ExitReadLock();
                }
            }
            return user;
        }

        public User? AddUser(string userName, string password)
        {
            User? user = null;
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                dbLock.EnterWriteLock();
                try
                {
                    using SQLCipher.Db db = new();

                    if ((db != null) && (db.Users != null))
                    {
                        if (db.Users.Where(o => o.UserName == userName).FirstOrDefault() == null)
                        {
                            // The User does not exist so add it
                            SQLCipher.User sqlCipherUser = new(userName, password);
                            db.Add(sqlCipherUser);
                            db.SaveChanges();
                            sqlCipherUser = db.Users.Where(o => o.UserName == userName).FirstOrDefault();
                            if (sqlCipherUser != null)
                            {
                                user = new(sqlCipherUser, false);
                            }
                        }
                    }
                }
                catch
                {
                    user = null;
                }
                finally
                {
                    dbLock.ExitWriteLock();
                }
            }
            return user;
        }

        public bool DelUser(ulong userId)
        {
            bool ok = false;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Users != null))
                {
                    SQLCipher.User user = db.Users.Where(o => o.Id == userId).FirstOrDefault();
                    if (user != null)
                    {
                        // The User exists so delete it
                        db.Remove(user);
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
                dbLock.ExitWriteLock();
            }
            return ok;
        }
        #endregion
        #region Roles

        public List<RoleData> GetRoles()
        {
            // Get the Roles
            List<RoleData> roles = new();
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Roles != null))
                {
                    db.Roles
                        //                        .Include(x => x.Users)
                        .ToList()
                        .ForEach(o => roles.Add(new(o, true)));
                }
            }
            catch
            {
                roles = new();
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return roles;
        }

        public RoleData? AddRole(string name)
        {
            RoleData? role = null;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Roles != null))
                {
                    // Check name is unique
                    SQLCipher.Role? sqlCipherRole = db.Roles
                      .Where(x => x.Name == name)
                      .FirstOrDefault();

                    // Add the role
                    if (sqlCipherRole == null)
                    {
                        sqlCipherRole = new();
                        sqlCipherRole.Name = name;

                        db.Roles.Add(sqlCipherRole);
                        db.SaveChanges();

                        sqlCipherRole = db.Roles
                           .Where(x => x.Name == name)
                          .FirstOrDefault();
                    }

                    role = new(sqlCipherRole, false);
                }
            }
            catch
            {
                role = null;
            }
            finally
            {
                dbLock.ExitWriteLock();
            }
            return role;
        }

        public bool DelRole(ulong roleId)
        {
            bool ok = false;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Roles != null))
                {
                    // Find the role
                    SQLCipher.Role? sqlCipherRole = db.Roles
                      .Include(o => o.Zones)
                      .Include(o => o.Users)
                      .Where(x => x.Id == roleId)
                      .FirstOrDefault();

                    // Delete the role
                    if (sqlCipherRole != null)
                    {
                        // Remove the Role's permission values
                        List<SQLCipher.DevPermVal>? sqlCipherdevPermVals = db.DevPermVals
                            .Where(x => x.Role == sqlCipherRole)
                            .ToList();
                        if (sqlCipherdevPermVals != null)
                        {
                            foreach (SQLCipher.DevPermVal sqlCipherDevPermVal in sqlCipherdevPermVals)
                            {
                                db.DevPermVals.Remove(sqlCipherDevPermVal);
                            }
                        }

                        // Delete the role
                        db.Roles.Remove(sqlCipherRole);
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
                dbLock.ExitWriteLock();
            }
            return ok;
        }

        #endregion
        #region Devs
        public List<Dev> GetDevs()
        {
            // Get the Devices
            List<Dev> devs = new();
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Devs != null))
                {
                    db.Devs
                        .Include(o => o.DevDef)
                        .Include(o => o.DevDef.DevPermDefs)
                        .Include(o => o.DevDef.DevPolDefs)
                        .Include(o => o.Zone)
                        .ToList()
                        .ForEach(o => devs.Add(new(o)));
                }
            }
            catch
            {
                devs = new();
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return devs;
        }

        public Dev? GetDev(ulong devId)
        {
            // Get the device type definition
            Dev? dev = null;
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Devs != null))
                {
                    SQLCipher.Dev sqlCipherDev = db.Devs
                      .Where(x => x.Id == devId)
                      .Include(x => x.Zone)
                      .Include(x => x.DevDef)
                      .Include(x => x.DevDef.DevPolDefs)
                      .Include(x => x.DevDef.DevPermDefs)
                      .FirstOrDefault();
                    if (sqlCipherDev != null)
                    {
                        dev = new(sqlCipherDev);
                    }
                }
            }
            catch
            {
                dev = null;
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return dev;
        }

        public Dev? AddDev(string devDefName, string devName)
        {
            Dev? dev = null;
            // Check parameters are valid
            devDefName = devDefName.Trim();
            devName = devName.Trim();
            if (!string.IsNullOrEmpty(devDefName) && !string.IsNullOrEmpty(devName))
            {
                dbLock.EnterWriteLock();
                try
                {
                    using SQLCipher.Db db = new();

                    if ((db != null) && (db.Devs != null))
                    {
                        // Check the device name is unique (Case insensitive)
                        SQLCipher.Dev sqlCipherDev = db.Devs
                          .Where(x => x.Name.ToUpper() == devName.ToUpper())
                           .FirstOrDefault();
                        if (sqlCipherDev == null)
                        {
                            // Check the DevDef name is valid
                            SQLCipher.DevDef sqlCipherDevDef = db.DevDefs
                              .Include(o => o.DevPolDefs)
                              .Include(o => o.DevPermDefs)
                              .Include(o => o.Langs)
                              .Where(x => x.Name.ToUpper() == devDefName.ToUpper())
                              .FirstOrDefault();

                            if (sqlCipherDevDef != null)
                            {
                                //if we are adding an App make sure it is a singleton
                                if (sqlCipherDevDef.App)
                                {
                                    // All Apps are singletons
                                    sqlCipherDev = db.Devs
                                        .Include(o => o.DevDef)
                                        .Where(x => x.DevDef.Name.ToUpper() == sqlCipherDevDef.Name.ToUpper())
                                        .FirstOrDefault();
                                }
                                if (sqlCipherDev == null)
                                {
                                    sqlCipherDev = new()
                                    {
                                        Name = devName,
                                        DevDef = sqlCipherDevDef
                                    };
                                    db.Add(sqlCipherDev);
                                    dev = new(sqlCipherDev);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    dev = null;
                }
                finally
                {
                    dbLock.ExitWriteLock();
                }
            }
            return dev;
        }


        public ReturnCode DelDev(ulong devId)
        {
            // Initialize return
            ReturnCode returnCode = ReturnCode.Unknown;

            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Devs != null))
                {
                    // Check device exists
                    SQLCipher.Dev sqlCipherDev = db.Devs
                      .Include(o => o.DevDef)
                      .Where(x => x.Id == devId)
                       .FirstOrDefault();
                    if (sqlCipherDev != null)
                    {
                        // Check it is not SecMan
                        if (sqlCipherDev.DevDef.Name.ToUpper() == "SECMAN")
                        {
                            // SecMan cannot be deleted
                            returnCode = ReturnCode.InvalidDevice;
                        }
                        else
                        {
                            db.Remove(sqlCipherDev);
                            db.SaveChanges();
                            returnCode = ReturnCode.Ok;
                        }
                    }
                }
            }
            catch
            {
                returnCode = ReturnCode.Unknown;
            }
            finally
            {
                dbLock.ExitWriteLock();
            }
            return returnCode;
        }


        //public List<DevPolVal>? GetDevPolVals(string name)
        //{
        //    // Get the device type definition
        //    List<DevPolVal> devPolVals = new();
        //    dbLock.EnterReadLock();
        //    try
        //    {
        //        using SQLCipher.Db db = new();

        //        if ((db != null) && (db.Devs != null))
        //        {
        //            SQLCipher.Dev dev = db.Devs
        //              .Where(x => x.Name == name)
        //              .Include(x => x.Zone)
        //              .Include(x => x.DevDef)
        //              .Include(x => x.DevDef.DevPolDefs)
        //              .FirstOrDefault();
        //            if (dev != null)
        //            {
        //                //List<SQLCipher.DevPolVal> devPolValsAll = db.DevPolVals.ToList();
        //                //devPolValsAll.Where(o => o.Zone == dev.Zone && o.DevDef == dev.DevDef).ToList().ForEach(o => devPolVals.Add(o));
        //                //List<SQLCipher.DevPolVal> devPolValsAll = db.DevPolVals.ToList();
        //                db.DevPolVals.Where(o => o.Zone == dev.Zone && o.DevDef == dev.DevDef).ToList().ForEach(o => devPolVals.Add(new(o)));
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        devPolVals = null;
        //    }
        //    finally
        //    {
        //        dbLock.ExitReadLock();
        //    }
        //    return devPolVals;
        //}

        //public List<DevPermVal>? GetDevPermVals(string roleName, string devName)
        //{
        //    // Get the device type definition
        //    List<DevPermVal> devPermVals = new();
        //    dbLock.EnterReadLock();
        //    try
        //    {
        //        using SQLCipher.Db db = new();
        //        SQLCipher.Dev sqLCipherDev = db.Devs
        //          .Where(x => x.Name == devName)
        //          .Include(x => x.Zone)
        //          .Include(x => x.DevDef)
        //          .FirstOrDefault();

        //        if ((sqLCipherDev != null) && (sqLCipherDev.Zone != null))
        //        {
        //            db.DevPermVals
        //                .Include(o => o.DevPermDef)
        //                .Where(o => o.Zone == sqLCipherDev.Zone && o.DevDef == sqLCipherDev.DevDef && o.Role.Name == roleName)
        //                .ToList()
        //                .ForEach(o => devPermVals.Add(new(o)));
        //        }
        //    }
        //    finally
        //    {
        //        dbLock.ExitReadLock();
        //    }
        //    return devPermVals;
        //}


        #endregion
        #region Zones

        public Zone? AddZone(string name)
        {
            name = name.Trim();
            Zone? zone = null;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Zones != null))
                {
                    // Check name is unique
                    SQLCipher.Zone? sqlCipherZone = db.Zones
                      .Where(x => x.Name.ToUpper() == name.ToUpper())
                      .FirstOrDefault();

                    // Add the role
                    if (sqlCipherZone == null)
                    {
                        sqlCipherZone = new();
                        sqlCipherZone.Name = name;

                        db.Zones.Add(sqlCipherZone);
                        db.SaveChanges();

                        sqlCipherZone = db.Zones
                           .Where(x => x.Name == name)
                          .FirstOrDefault();
                    }

                    zone = new(sqlCipherZone, true);
                }
            }
            catch
            {
                zone = null;
            }
            finally
            {
                dbLock.ExitWriteLock();
            }
            return zone;
        }

        public bool DelZone(ulong zoneId)
        {
            bool ok = false;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Zones != null))
                {
                    // Find the zone
                    SQLCipher.Zone? sqlCipherZone = db.Zones
                      .Include(o => o.Roles)
                      .Where(x => x.Id == zoneId)
                      .FirstOrDefault();

                    // Remove the Zone's device definition policy values
                    List<SQLCipher.DevPolVal>? sqlCipherDevPolVals = db.DevPolVals
                        .Where(x => x.Zone == sqlCipherZone)
                        .ToList();
                    if (sqlCipherDevPolVals != null)
                    {
                        foreach (SQLCipher.DevPolVal sqlCipherDevPolVal in sqlCipherDevPolVals)
                        {
                            db.DevPolVals.Remove(sqlCipherDevPolVal);
                        }
                    }

                    // Remove the Zone's device definition signature  values
                    List<SQLCipher.DevSigVal>? sqlCipherDevSigVals = db.DevSigVals
                        .Where(x => x.Zone == sqlCipherZone)
                        .ToList();
                    if (sqlCipherDevSigVals != null)
                    {
                        foreach (SQLCipher.DevSigVal sqlCipherDevSigVal in sqlCipherDevSigVals)
                        {
                            db.DevSigVals.Remove(sqlCipherDevSigVal);
                        }
                    }

                    // Remove the Zone's permission values
                    List<SQLCipher.DevPermVal>? sqlCipherDevPermVals = db.DevPermVals
                        .Where(x => x.Zone == sqlCipherZone)
                        .ToList();
                    if (sqlCipherDevPermVals != null)
                    {
                        foreach (SQLCipher.DevPermVal sqlCipherDevPermVal in sqlCipherDevPermVals)
                        {
                            db.DevPermVals.Remove(sqlCipherDevPermVal);
                        }
                    }

                    // Deallocate devices
                    db.Devs
                       .Include(o => o.Zone)
                       .Where(o => o.Zone == sqlCipherZone)
                       .ToList()
                       .ForEach(o => o.Zone = null);

                    // Delete the role
                    if (sqlCipherZone != null)
                    {
                        db.Zones.Remove(sqlCipherZone);
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
                dbLock.ExitWriteLock();
            }
            return ok;
        }


        public List<Zone> GetZones()
        {
            // Get the Roles
            List<Zone> zones = new();
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Zones != null))
                {
                    List<SQLCipher.Zone> sqlCipherZones = db.Zones
                        .Include(o => o.Roles)
                        .ToList();

                    foreach (SQLCipher.Zone sqlCipherZone in sqlCipherZones)
                    {
                        Zone zone = new Zone(sqlCipherZone, true);
                        db.Devs
                            .Include(o => o.DevDef)
                            .Where(o => o.Zone != null && o.Zone == sqlCipherZone)
                            .ToList()
                            .ForEach(o => zone.Devs.Add(new(o)));
                        zones.Add(zone);
                    }
                }
            }
            catch
            {
                zones = new();
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return zones;
        }

        //public Zone? GetZone(ulong zoneId)
        //{
        //    // Get the device type definition
        //    Zone? zone = null;
        //    dbLock.EnterReadLock();
        //    try
        //    {
        //        using SQLCipher.Db db = new();

        //        if ((db != null) && (db.Zones != null))
        //        {
        //            zone = new(db.Zones
        //              .Where(x => x.Id == zoneId)
        //              .FirstOrDefault(), true);

        //        }
        //    }
        //    catch
        //    {
        //        zone = null;
        //    }
        //    finally
        //    {
        //        dbLock.ExitReadLock();
        //    }
        //    return zone;
        //}

        //public List<Dev>? GetZoneDevs(ulong zoneId)
        //{
        //    // Get the device type definition
        //    List<Dev>? devs = [];
        //    dbLock.EnterReadLock();
        //    try
        //    {
        //        using SQLCipher.Db db = new();

        //        if ((db != null) && (db.Devs != null))
        //        {
        //            db.Devs
        //              .Include(x => x.Zone)
        //              .Include(x => x.DevDef)
        //              .Include(x => x.DevDef.DevPolDefs)
        //              .Include(x => x.DevDef.DevPermDefs)
        //              .Where(x => x.Zone.Id == zoneId)
        //              .ToList().ForEach(o => devs.Add(new(o)));
        //        }
        //    }
        //    catch
        //    {
        //        devs = [];
        //    }
        //    finally
        //    {
        //        dbLock.ExitReadLock();
        //    }
        //    return devs;
        //}

        //public List<Role>? GetZoneRoles(ulong zoneId)
        //{
        //    // Get the device type definition
        //    List<Role>? roles = new();
        //    dbLock.EnterReadLock();
        //    try
        //    {
        //        using SQLCipher.Db db = new();

        //        if ((db != null) && (db.Zones != null))
        //        {
        //            SQLCipher.Zone zone = db.Zones
        //                .Where(x => x.Id == zoneId)
        //                .Include(x => x.Roles)
        //                .FirstOrDefault();

        //            if (zone != null)
        //            {
        //                zone.Roles.ForEach(o => roles.Add(new(o, false)));
        //            }

        //        }
        //    }
        //    catch
        //    {
        //        roles = null;
        //    }
        //    finally
        //    {
        //        dbLock.ExitReadLock();
        //    }
        //    return roles;
        //}
        #endregion
        #region SysFeats

        public List<SysFeat> GetSysFeats(string langCode)
        {
            // Get the System Common features
            List<SysFeat> sysFeats = new();
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.SysFeats != null))
                {
                    db.SysFeats
                       .Include(o => o.Langs)
                       .Include(o => o.SysFeatProps)
                       .ThenInclude(p => p.Langs)
                       .ToList()
                       .ForEach(o => sysFeats.Add(new(langCode, o)));
                }
            }
            catch
            {
                sysFeats = new();
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return sysFeats;
        }

        public bool SetSysFeatPropVal(ulong sysFeatPropId, string val)
        {
            bool ok = false;
            dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.SysFeatProps != null))
                {
                    SQLCipher.SysFeatProp sysFeatProp = db.SysFeatProps
                       .Where(o => o.Id == sysFeatPropId)
                       .FirstOrDefault();
                    if (sysFeatProp != null)
                    {
                        sysFeatProp.Val = val;
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
                dbLock.ExitWriteLock();
            }
            return ok;
        }

        #endregion
        #region DevDefs

        public List<DevDef> GetDevDefs(string langCode)
        {
            // Get the System Common features
            List<DevDef> devDefs = new();
            dbLock.EnterReadLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.SysFeats != null))
                {
                    db.DevDefs
                       .Include(o => o.Langs)
                       .Include(o => o.DevPolDefs)
                       .ThenInclude(p => p.Langs)
                       .Include(o => o.DevPermDefs)
                       .ThenInclude(p => p.Langs)
                       .ToList()
                       .ForEach(o => devDefs.Add(new(langCode, o)));
                }
            }
            catch
            {
                devDefs = new();
            }
            finally
            {
                dbLock.ExitReadLock();
            }
            return devDefs;
        }
        #endregion

        public ReturnCode SetDevPolVal(ulong devPolValId, string val)
        {
            ReturnCode retCode = ReturnCode.Ok;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                SQLCipher.DevPolVal? sqlCipherDevPolVal = db.DevPolVals
                   .Where(o => o.Id == devPolValId)
                   .FirstOrDefault();
                if (sqlCipherDevPolVal != null)
                {
                    sqlCipherDevPolVal.Val = val;
                    db.SaveChanges();
                }
                else
                {
                    retCode = ReturnCode.NonExistantObject;
                }
            }
            catch
            {
                retCode = ReturnCode.Unknown;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return retCode;
        }
        public ReturnCode SetSigVal(ulong sigValId, bool sign, bool auth, bool note)
        {
            ReturnCode retCode = ReturnCode.Ok;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                SQLCipher.DevSigVal? sqlCipherDevSigVal = db.DevSigVals
                   .Where(o => o.Id == sigValId)
                   .FirstOrDefault();
                if (sqlCipherDevSigVal != null)
                {
                    sqlCipherDevSigVal.Sign = sign;
                    sqlCipherDevSigVal.Auth = auth;
                    sqlCipherDevSigVal.Note = note;
                    db.SaveChanges();
                }
                else
                {
                    retCode = ReturnCode.NonExistantObject;
                }
            }
            catch
            {
                retCode = ReturnCode.Unknown;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return retCode;
        }
        public ReturnCode SetPermVal(ulong permValId, bool val)
        {
            ReturnCode retCode = ReturnCode.Ok;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();
                SQLCipher.DevPermVal? sqlCipherDevPermVal = db.DevPermVals
                   .Where(o => o.Id == permValId)
                   .FirstOrDefault();
                if (sqlCipherDevPermVal != null)
                {
                    sqlCipherDevPermVal.Val = val;
                    db.SaveChanges();
                }
                else
                {
                    retCode = ReturnCode.NonExistantObject;
                }
            }
            catch
            {
                retCode = ReturnCode.Unknown;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return retCode;
        }


        /// <summary>
        /// Loads the application launcher initialization data from a JSON file and updates the database with the installed applications.
        /// The method checks for each application in the JSON file:
        /// 1. If it exists in the database, it updates the `InstalledDate`.
        /// 2. If it does not exist, it adds a new record.
        /// </summary>
        /// <param name="db">The database context used to access the `Applications` table.</param>
        /// <param name="filePath">The file path of the JSON file containing the application launcher data.</param>
        /// <returns>
        /// Returns `true` if the JSON file was successfully loaded and the applications were added/updated in the database.
        /// Returns `false` if an error occurred or if no data was processed.
        /// </returns>
        internal bool LoadApplicationLauncherInitFile(SQLCipher.Db db, string filePath)
        {
            bool ok = false; // Variable to indicate success or failure
            try
            {
                // Read the JSON file
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd(); // Read the entire content of the file
                    Model.Applications applications = JsonSerializer.Deserialize<Model.Applications>(json); // Deserialize JSON into the Applications model

                    // Check if deserialization was successful and if we have applications to process
                    if (applications != null)
                    {
                        // Iterate over each app in the list of installed applications
                        foreach (var app in applications.InstalledApps)
                        {
                            // Check if the application already exists in the database by its name
                            var existingApp = db.Applications.FirstOrDefault(a => a.Name == app.Name);

                            if (existingApp != null)
                            {
                                // Update the existing application's installed date
                                existingApp.InstalledDate = app.InstalledOn;
                            }
                            else
                            {
                                // Add the new application to the database
                                db.Applications.Add(new SQLCipher.Applications
                                {
                                    Name = app.Name,
                                    InstalledDate = app.InstalledOn
                                });
                            }
                        }

                        // Save all changes to the database
                        db.SaveChanges();
                        ok = true; // Set the success flag to true
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception (if needed)
            }

            return ok; // Return the success or failure status
        }



        internal bool LoadRsaKeys(SQLCipher.Db db, string filePath)
        {
            bool ok = false;
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    SQLCipher.RSAKeys? key = JsonSerializer.Deserialize<SQLCipher.RSAKeys>(json);
                    if ((key != null) && (key.PrivateKey != null))
                    {
                        // Does the Dev Type Definition already exist
                        if (db.RSAKeys.Where(x => x.PrivateKey == key.PrivateKey).FirstOrDefault() == null)
                        {
                            // The Device Type does not exist so add it
                            db.Add(key);
                            db.SaveChanges();
                        }
                        ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return ok;
        }

    }
}
