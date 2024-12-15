using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecMan.Data.Exceptions;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.DAL;
using SecMan.Model;

namespace SecMan.Data
{
    public class Role : IRoleDal
    {
        SecManDb db = new();
        #region JohnCode
        internal Role(SQLCipher.Role role, bool includeUsers)
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

        private readonly ILogger<Role> _logger;

        private readonly Db _context;
        public Role(ILogger<Role> logger)
        {
            _logger = logger;
        }
        internal Role(Db context)
        {
        }

        public async Task<GetRoleDto> AddRoleAsync(CreateRole addRoleDto)
        {
            _logger.LogInformation("Starting to add role. Role Name: {RoleName}", addRoleDto.Name);

            using var db = new SQLCipher.Db();
            var existingRole = await db.Roles
                .AnyAsync(x => x.Name == addRoleDto.Name);

            if (existingRole)
            {
                _logger.LogWarning("Conflict detected. Role with name {RoleName} already exists.", addRoleDto.Name);
                throw new ConflictException("A role with the same name already exists.");
            }
            var allUsers = await db.Users.CountAsync(x => addRoleDto.LinkUsers.Contains(x.Id));

            if (allUsers != addRoleDto.LinkUsers.Count)
            {
                _logger.LogWarning("Invalid user IDs provided for role addition");
                throw new BadRequestForLinkUsersNotExits("Some provided user IDs do not exist. Please provide valid user IDs.");
            }
            try
            {
                var validUsers = await db.Users
                                .Where(x => addRoleDto.LinkUsers.Contains(x.Id))
                                .ToListAsync();
                // Create new role
                var role = new SecMan.Data.SQLCipher.Role
                {
                    Name = addRoleDto.Name,
                    Description = addRoleDto.Description,
                    IsLoggedOutType = addRoleDto.IsLoggedOutType ?? false,
                    Users = validUsers
                };

                _logger.LogInformation("Adding new role to database. Role Name: {RoleName}", addRoleDto.Name);

                await db.Roles.AddAsync(role);
                await db.SaveChangesAsync();

                ulong roleId = role.Id;

                _logger.LogInformation("Role added successfully with ID: {RoleId}", roleId);

                // Create DTO for the new role
                var retGetRoleDto = new GetRoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsLoggedOutType = role.IsLoggedOutType ?? false,
                    NoOfUsers = addRoleDto.LinkUsers.Count
                };

                _logger.LogInformation("Role creation completed. Role ID: {RoleId}", roleId);

                return retGetRoleDto;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception occurred while adding role. Role Name: {RoleName}", addRoleDto.Name);
                throw new ApplicationException("An error occurred while updating the database.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred while adding role. Role Name: {RoleName}", addRoleDto.Name);
                throw new ApplicationException("An unexpected error occurred while adding the role.", ex);
            }
        }
        public async Task<List<GetRoleDto>> GetAllRolesAsync()
        {
            _logger.LogInformation("Starting to retrieve all roles.");

            try
            {
                using var db = new SQLCipher.Db();

                _logger.LogInformation("Querying database to get roles and associated user counts.");

                var getRoles = await db.Roles
                    .Select(r => new GetRoleDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsLoggedOutType = r.IsLoggedOutType ?? false,
                        NoOfUsers = r.Users.Count
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {RoleCount} roles from the database.", getRoles.Count);

                return getRoles;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception occurred while retrieving roles.");
                throw new ApplicationException("An error occurred while retrieving roles from the database.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred while retrieving roles.");
                throw new ApplicationException("An unexpected error occurred while retrieving roles.", ex);
            }
        }
        public async Task<GetRoleDto?> GetRoleByIdAsync(ulong id)
        {
            _logger.LogInformation("Attempting to retrieve role with ID: {RoleId}", id);

            try
            {
                using var db = new SQLCipher.Db();

                _logger.LogInformation("Querying database for role with ID: {RoleId}", id);

                // Retrieve the role including associated users
                var role = await db.Roles.Include(x => x.Users).FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    _logger.LogInformation("No role found with ID: {RoleId}.", id);
                    return null;
                }

                // Create the DTO to return
                var getRoleDto = new GetRoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsLoggedOutType = role.IsLoggedOutType ?? false,
                    NoOfUsers = role.Users.Count
                };

                _logger.LogInformation("Successfully retrieved role with ID: {RoleId}. Number of associated users: {UserCount}", id, role.Users.Count);

                return getRoleDto;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception occurred while retrieving role with ID: {RoleId}.", id);
                throw new ApplicationException("An error occurred while retrieving the role from the database.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred while retrieving role with ID: {RoleId}.", id);
                throw new ApplicationException("An unexpected error occurred while retrieving the role by ID.", ex);
            }
        }
        public async Task<GetRoleDto?> UpdateRoleAsync(ulong id, CreateRole addRoleDto)
        {
            _logger.LogInformation("Attempting to update role with ID: {RoleId}", id);

            using var db = new SQLCipher.Db();

            _logger.LogInformation("Querying database for role with ID: {RoleId}", id);

            // Retrieve the role to be updated
            var role = await db.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);

            if (role == null)
            {
                _logger.LogInformation("No role found with ID: {RoleId}. Update aborted.", id);
                return null;
            }
            var existingName = await db.Roles
                .AnyAsync(x => x.Name == addRoleDto.Name && x.Id != id);

            if (existingName)
            {
                _logger.LogWarning("Conflict detected. Role with name {RoleName} already exists.", addRoleDto.Name);
                throw new UpdatingExistingNameException("A role with the same name already exists.");
            }

            var allUsers = await db.Users.Where(x => addRoleDto.LinkUsers.Contains(x.Id))
                                          .ToListAsync();
            if (allUsers.Count != addRoleDto.LinkUsers.Count)
            {
                _logger.LogWarning("Invalid user IDs provided for role addition");
                throw new BadRequestForLinkUsersNotExits("Some provided user IDs do not exist. Please provide valid user IDs.");
            }

            try
            {
                _logger.LogInformation("Role found with ID: {RoleId}. Updating role details.", id);

                // Update role properties
                role.Name = addRoleDto.Name ?? role.Name;
                role.Description = addRoleDto.Description ?? role.Description;
                role.IsLoggedOutType = addRoleDto.IsLoggedOutType ?? role.IsLoggedOutType;

                // Update role-user links
                if (addRoleDto.LinkUsers != null)
                {
                    var newUsers = allUsers.Where(x => addRoleDto.LinkUsers.Contains(x.Id)).ToList();
                    role.Users = newUsers;
                }

                _logger.LogInformation("Removing existing role-user links and adding new ones for Role ID: {RoleId}.", id);

                // Save changes to the database
                db.Roles.Update(role);
                await db.SaveChangesAsync();

                _logger.LogInformation("Role updated successfully with ID: {RoleId}.", id);

                // Create DTO for updated role
                var updatedRoleDto = new GetRoleDto
                {
                    Id = id,
                    Name = role.Name,
                    Description = role.Description,
                    IsLoggedOutType = role.IsLoggedOutType ?? false,
                    NoOfUsers = role.Users.Count
                };

                _logger.LogInformation("Role update completed with ID: {RoleId}. New user count: {UserCount}", id, updatedRoleDto.NoOfUsers);

                return updatedRoleDto;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception occurred while updating role with ID: {RoleId}", id);
                throw new ApplicationException("An error occurred while updating the role in the database.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred while updating role with ID: {RoleId}", id);
                throw new ApplicationException("An unexpected error occurred while updating the role.", ex);
            }
        }
        public async Task<bool> DeleteRoleAsync(ulong id)
        {
            _logger.LogInformation("Attempting to delete role with ID: {RoleId}", id);

            try
            {
                using var db = new SQLCipher.Db();

                _logger.LogInformation("Querying database for role with ID: {RoleId}", id);

                // Retrieve the role to be deleted
                var role = await db.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);

                if (role == null)
                {
                    _logger.LogInformation("No role found with ID: {RoleId}. Deletion aborted.", id);
                    return false;
                }

                _logger.LogInformation("Role found with ID: {RoleId}. Proceeding to delete role and associated role-user links.", id);

                // Remove associated role-user links if necessary
                if (role.Users.Any())
                {
                    _logger.LogInformation("Removing associated role-user links for Role ID: {RoleId}. Number of links to remove: {LinkCount}", id, role.Users.Count);
                }

                // Remove the role
                db.Roles.Remove(role);

                // Save changes to the database
                await db.SaveChangesAsync();

                _logger.LogInformation("Role with ID: {RoleId} deleted successfully.", id);

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception occurred while deleting role with ID: {RoleId}.", id);
                throw new ApplicationException("An error occurred while deleting the role from the database.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occurred while deleting role with ID: {RoleId}.", id);
                throw new ApplicationException("An unexpected error occurred while deleting the role.", ex);
            }
        }
    }
}
