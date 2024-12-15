using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.Model.Common;
using Serilog;

namespace SecMan.Data.DAL
{
    public class UserRepository : GenericRepository<SQLCipher.User>, IUserRepository
    {
        private readonly Serilog.ILogger _logger = Log.ForContext<UserRepository>();
        private readonly Db _context;

        public UserRepository(Db context) : base(context)
        {
            _context = context;
        }

        public async Task<Model.User> UpdateUserAsync(UpdateUser model, ulong userId, DateTime? passwordExpiryDate = null)
        {
            _logger.Information("Fetch user object by user Id : {@UserId} to update", userId);

            SQLCipher.User user = await _context.Users
                                     .Where(x => x.Id == userId)
                                     .Include(x => x.Roles)
                                     .FirstOrDefaultAsync();

            if (user is null)
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            user.Description = model.Description ?? user.Description;
            user.Domain = model.Domain ?? user.Domain;
            user.Email = model.Email ?? user.Email;
            user.FirstName = model.FirstName ?? user.FirstName;
            user.Language = model.Language ?? user.Language;
            user.LastName = model.LastName ?? user.LastName;
            user.PasswordExpiryDate = passwordExpiryDate ?? user.PasswordExpiryDate;
            user.PasswordExpiryEnable = model.IsPasswordExpiryEnabled ?? user.PasswordExpiryEnable;
            user.UserName = model.Username ?? user.UserName;
            user.IsActive = model.IsActive ?? user.IsActive;
            user.ResetPassword = model.ResetPassword ?? user.ResetPassword;
            user.Password = model.Password ?? user.Password;
            user.FirstLogin = model.FirstLogin ?? user.FirstLogin;
            if (model.IsLegacy != null && model.IsLegacy == true && !user.IsLegacy)
            {
                user.FirstLogin = true;
            }
            user.IsLegacy = model.IsLegacy ?? user.IsLegacy;
            if (model.Roles != null)
            {
                Task<List<SQLCipher.Role>> roles = _context.Roles.Where(x => model.Roles.Contains(x.Id)).ToListAsync();
                user.Roles = roles.Result;
            }
            _logger.Information("Validating Add User Data");
            user = ValidateAddUserDto(user);

            _context.Update(user);

            if (model.Password != null)
            {
                await InsertPasswordHistoryAsync(userId, model.Password, DateTime.UtcNow);
            }

            return new Model.User()
            {
                IsActive = user.IsActive,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,
                FirstName = user.FirstName,
                Language = user.Language,
                LastName = user.LastName,
                IsLegacy = user.IsLegacy,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                IsRetired = user.Retired,
                LastLogin = user.LastLoginDate,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Roles = user.Roles.Select(x => new Model.RoleModel
                {
                    Description = x.Description,
                    Id = x.Id,
                    IsLoggedOutType = x.IsLoggedOutType,
                    Name = x.Name,
                }).ToList(),
                Username = user.UserName,
            };
        }


        private async Task InsertPasswordHistoryAsync(ulong userId, string password, DateTime changeDate)
        {
            PasswordHistory passwordHistory = new PasswordHistory
            {
                UserId = userId,
                Password = password,
                CreatedDate = changeDate
            };
            await _context.PasswordHistories.AddAsync(passwordHistory);
        }

        private static SQLCipher.User ValidateAddUserDto(SQLCipher.User model)
        {
            if (!model.IsActive)
                model.InActiveDate = DateTime.UtcNow;

            if (model.Locked)
                model.LockedDate = DateTime.UtcNow;

            if (model.Retired)
                model.RetiredDate = DateTime.UtcNow;

            return model;
        }


        public async Task<List<RoleModel>> GetRolesByRoleId(List<ulong> roleIds)
        {
            IQueryable<SQLCipher.Role> result = _context.Roles.Where(x => roleIds.Contains(x.Id));

            return await result.Select(r => new RoleModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsLoggedOutType = r.IsLoggedOutType,
                NoOfUsers = r.Users.Count
            }).ToListAsync();
        }


        public async Task<Model.User> AddUserAsync(CreateUser model)
        {
            _logger.Information("Mapping data to dbset user object");
            SQLCipher.User user = new SQLCipher.User
            {
                Description = model.Description,
                Domain = model.Domain,
                Email = model.Email,
                FirstName = model.FirstName,
                Language = model.Language,
                LastName = model.LastName,
                IsLegacy = model.IsLegacy,
                Password = model.Password,
                PasswordExpiryEnable = model.IsPasswordExpiryEnabled,
                UserName = model.Username,
                IsActive = model.IsActive,
                FirstLogin = model.FirstLogin,
                ResetPassword = model.ResetPassword,
                PasswordDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            if (model.IsPasswordExpiryEnabled)
            {
                SQLCipher.SysFeat sysfeat = await _context.SysFeats.Include(x => x.SysFeatProps).Where(x => x.Id == 3).FirstOrDefaultAsync();
                SQLCipher.SysFeatProp sysProp = sysfeat.SysFeatProps.Where(x => x.Name == SysFeatureConstants.Expiry).FirstOrDefault();
                int passwordExpiryPeriod = Convert.ToInt32(sysProp.Val) > 0 ? Convert.ToInt32(sysProp.Val) : 30;
                user.PasswordExpiryDate = DateTime.UtcNow.AddDays(passwordExpiryPeriod);
            }

            _logger.Information("fetching all the roles which are need to be associated with user");
            List<SQLCipher.Role> roles = await _context.Roles.Where(x => model.Roles.Contains(x.Id)).Include(x => x.Users).ToListAsync();

            user.Roles = roles;

            _logger.Information("Validating Boolean values and assigning its related dates");
            user = ValidateAddUserDto(user);

            user.PasswordHistories = new List<PasswordHistory>
            {
                new PasswordHistory
                {
                    Password = model.Password,
                    CreatedDate = DateTime.Now,
                }
            };

            await _context.AddAsync(user);

            return new Model.User()
            {
                IsActive = user.IsActive,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,
                FirstName = user.FirstName,
                Language = user.Language,
                LastName = user.LastName,
                IsLegacy = user.IsLegacy,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                IsRetired = user.Retired,
                LastLogin = user.LastLoginDate,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Roles = user.Roles.Select(x => new Model.RoleModel
                {
                    Description = x.Description,
                    Id = x.Id,
                    IsLoggedOutType = x.IsLoggedOutType,
                    Name = x.Name,
                    NoOfUsers = x.Users.Count
                }).ToList(),
                Username = user.UserName,
            };
        }

        public async Task<SecMan.Data.SQLCipher.User> GetUserByUsername(string? username)
        {
            if (username != null)
            {
                return await _context.Users.Where(x => x.UserName != null &&
                                                  x.UserName.ToLower().Equals(username.ToLower()))
                                                  .FirstOrDefaultAsync();
            }
            return null;
        }


        public async Task UpdateUserSessionDetails(ulong userId, string sessionId, double sessionExpiryTime)
        {
            SQLCipher.User user = await _context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                user.SessionId = sessionId;
                user.LastLoginDate = DateTime.UtcNow;
                user.SessionExpiry = DateTime.UtcNow.AddMinutes(sessionExpiryTime);
                // Check if the first login is true, since updating session info the user is loged in already so updating this bool to false in below if condition
                if (user.FirstLogin)
                {
                    user.FirstLogin = false;
                }
                _context.Update(user);
            }
        }


        public async Task<Tuple<UserDetails?, List<AppPermissions>?>> GetUserDetails(ulong userId)
        {
            UserDetails userDetails = new UserDetails();
            List<AppPermissions> permissions = new List<AppPermissions>();
            SQLCipher.User user = await _context.Users
                                .Include(x => x.Roles)
                                .Where(x => x.Id == userId)
                                .FirstOrDefaultAsync();
            if (user is not null)
            {
                userDetails = new UserDetails
                {
                    IsActive = user.IsActive,
                    Description = user.Description,
                    Domain = user.Domain,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Id = user.Id,
                    IsLegacy = user.IsLegacy,
                    IsLocked = user.Locked,
                    IsRetired = user.Retired,
                    Language = user.Language,
                    LastName = user.LastName,
                    Roles = user.Roles.Select(x => x.Name).ToList(),
                    Username = user.UserName,
                    FirstLogin = user.FirstLogin,
                };
                if (user.PasswordExpiryEnable && user.PasswordExpiryDate < DateTime.UtcNow)
                {
                    userDetails.IsExpired = true;
                }
                else
                {
                    userDetails.IsExpired = false;
                }

                foreach (KeyValuePair<string, List<Permission>> p in GetAppPerms(userId))
                {
                    AppPermissions permission = new AppPermissions
                    {
                        Name = p.Key,
                        Permissions = p.Value
                    };
                    permissions.Add(permission);
                }
            }
            else
            {
                userDetails = default;
                permissions = default;
            }
            return new Tuple<UserDetails?, List<AppPermissions>?>(userDetails, permissions);
        }


        public async Task<SecMan.Data.SQLCipher.User> GetUserById(ulong id)
        {
            return await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ulong> GetUserBySessionId(string sessionId)
        {
            ulong user = await _context.Users.Where(x => x.SessionId == sessionId &&
                                                      x.SessionExpiry > DateTime.UtcNow)
                                          .Select(x => x.Id)
                                          .FirstOrDefaultAsync();
            return user;
        }




        public async Task RetireUserAsync(SQLCipher.User user)
        {
            Log.Information($"Checking if user is already retired");
            if (!user.Retired)
            {
                user.Retired = true;
                user.RetiredDate = DateTime.UtcNow;
                user.Roles = [];
            }
            Log.Information("Saving User by updated retired bool and its date");
            _context.Update(user);
        }

        public Dictionary<string, List<Permission>> GetAppPerms(ulong userId)
        {
            Dictionary<string, List<Permission>> userAppPerms = new();

            // Get the user and roles
            SQLCipher.User sqlCipherUser = _context.Users
                .Include(x => x.Roles)
                .Where(o => o.Id == userId)
                .FirstOrDefault();
            if (sqlCipherUser != null)
            {
                // Get the Zones the Role is allocated to
                if (sqlCipherUser.Roles != null)
                {
                    foreach (SQLCipher.Role sqlCipheRole in sqlCipherUser.Roles)
                    {
                        List<SQLCipher.Zone>? sqlCipherZones = null;
                        if (_context.Zones != null)
                        {
                            sqlCipherZones = _context.Zones.Where(o => o.Roles.Contains(sqlCipheRole)).ToList();
                            if (sqlCipherZones != null)
                            {
                                List<SQLCipher.Dev>? sqlCipherDevs = null;
                                foreach (SQLCipher.Zone sqlCipherZone in sqlCipherZones)
                                {
                                    // Get the App Devices allocated to the Zone
                                    sqlCipherDevs = _context.Devs
                                        .Include(o => o.DevDef)
                                        .Include(o => o.Zone)
                                        .Where(o => o.DevDef.App && o.Zone == sqlCipherZone).ToList();
                                    foreach (SQLCipher.Dev sqlCipherDev in sqlCipherDevs)
                                    {

                                        List<SQLCipher.DevPermVal>? devPermVals = _context.DevPermVals
                                            .Include(o => o.DevPermDef)
                                            .Where(o => o.Zone == sqlCipherZone && o.DevDef == sqlCipherDev.DevDef && o.Role == sqlCipheRole).ToList();

                                        if (devPermVals != null)
                                        {
                                            if (userAppPerms.ContainsKey(sqlCipherDev.Name))
                                            {
                                                foreach (SQLCipher.DevPermVal devPermVal in devPermVals)
                                                {
                                                    SQLCipher.DevSigVal signatures = _context.DevSigVals
                                                                    .Where(x => x.Zone == devPermVal.Zone)
                                                                    .Where(x => x.DevDef == devPermVal.DevDef)
                                                                    .Where(x => x.DevPermDef == devPermVal.DevPermDef)
                                                                    .FirstOrDefault();

                                                    if ((devPermVal.Val) && !userAppPerms[sqlCipherDev.Name].Select(x => x.Name).Contains(devPermVal.DevPermDef.Name))
                                                    {
                                                        userAppPerms[sqlCipherDev.Name].Add(
                                                            new Permission
                                                            {
                                                                Name = devPermVal.DevPermDef.Name,
                                                                Signatures = new Signatures
                                                                {
                                                                    Authorization = signatures?.Auth ?? false,
                                                                    Note = signatures?.Note ?? false,
                                                                    Signature = signatures?.Sign ?? false
                                                                }
                                                            });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                userAppPerms.Add(sqlCipherDev.Name, new());

                                                foreach (SQLCipher.DevPermVal devPermVal in devPermVals)
                                                {
                                                    SQLCipher.DevSigVal signatures = _context.DevSigVals
                                                                    .Where(x => x.Zone == devPermVal.Zone)
                                                                    .Where(x => x.DevDef == devPermVal.DevDef)
                                                                    .Where(x => x.DevPermDef == devPermVal.DevPermDef)
                                                                    .FirstOrDefault();
                                                    if (devPermVal.Val)
                                                    {
                                                        userAppPerms[sqlCipherDev.Name].Add(new Permission
                                                        {
                                                            Name = devPermVal.DevPermDef.Name,
                                                            Signatures = new Signatures
                                                            {
                                                                Authorization = signatures?.Auth ?? false,
                                                                Note = signatures?.Note ?? false,
                                                                Signature = signatures?.Sign ?? false
                                                            }
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return userAppPerms;
        }



        /// <summary>
        /// Removes the session associated with the specified sessionId.
        /// </summary>
        /// <param name="sessionId">The sessionId of the user whose session is to be removed.</param>
        /// <returns>
        /// A Task representing the asynchronous operation. Returns true if the session was successfully removed; otherwise, false.
        /// </returns>
        public async Task<bool> ClearUserSessionAsync(string sessionId)
        {
            Log.Information("Attempting to remove session for user in DAL: {SessionID}", sessionId);
            SQLCipher.User user = await _context.Users.FirstOrDefaultAsync(
                u => u.SessionId == sessionId
               );

            if (user is null)
            {
                Log.Warning("No user found with : {SessionID}", sessionId);
                return false;
            }

            // Update the session-related fields in UsersTable
            user.SessionId = string.Empty;
            user.LastLogoutDate = DateTime.UtcNow;

            Log.Information("Session removed and LastLogoutDate updated for user: {SessionID}", sessionId);

            return true;
        }
        public async Task UnlockUserAsync(SQLCipher.User user, bool changePasswordOnLogin)
        {
            if (user.Locked)
            {
                user.Locked = false;
                user.LockedDate = DateTime.MinValue;
                user.FirstLogin = changePasswordOnLogin;

            }
            _context.Update(user);
        }
        public async Task<bool> UpdateUserLanguageAsync(ulong userId, string language)
        {
            SQLCipher.User user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.Language = language;
            _context.Update(user);
            return true;
        }



        public override async Task<bool> Delete(object id)
        {
            var passwordHistory = await _context.PasswordHistories.Where(x => x.UserId == (ulong)id).ToListAsync();

            var user = await _context.Users.FindAsync(id);

            var eventLogs = await _context.EventLogs.Where(x => x.User == user || x.AuthorizingUser == user || x.SigningUser == user).ToListAsync();

            _context.RemoveRange(passwordHistory);
            _context.RemoveRange(eventLogs);

            return await base.Delete(id);
        }
    }
}
