using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using Serilog;

namespace SecMan.Data.DAL
{

    public class RoleRepository : GenericRepository<SQLCipher.Role>, IRoleRepository
    {
        private readonly Serilog.ILogger _logger = Log.ForContext<RoleRepository>();
        private readonly Db _context;

        public RoleRepository(Db context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> IsRoleNameExistsForCreationAsync(string name)
        {
            return await _context.Roles.AnyAsync(x => x.Name == name);
        }

        public async Task<bool> GetUserRetiredStatusAsync(ulong userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Retired)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ValidateLinkUsersAsync(List<ulong> linkUserIds)
        {
            var existingUsersCount = await _context.Users.CountAsync(x => linkUserIds.Contains(x.Id));
            return existingUsersCount == linkUserIds.Count;
        }
        public async Task<SQLCipher.Role> AddRoleAsync(CreateRole addRoleDto)
        {
            List<SQLCipher.User> validUsers = await _context.Users.Where(x => addRoleDto.LinkUsers.Contains(x.Id)).ToListAsync();
            SQLCipher.Role role = new SQLCipher.Role
            {
                Name = addRoleDto.Name,
                Description = addRoleDto.Description,
                IsLoggedOutType = addRoleDto.IsLoggedOutType ?? false,
                Users = validUsers,
                CreatedDate = DateTime.Now
            };
            await _context.Roles.AddAsync(role);
            return role;
        }
        public async Task<bool> IsRoleNameExistsAsync(ulong id, string name)
        {
            return await _context.Roles.AnyAsync(x => x.Name == name && x.Id != id);
        }
        public async Task<GetRoleDto> UpdateRoleFromJsonAsync(ulong id, UpdateRole addRoleDto)
        {
            SQLCipher.Role role = await _context.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);

            if (role == null)
            {
                return null;
            }
            List<SQLCipher.User> allUsers = await _context.Users.Where(x => addRoleDto.LinkUsers.Contains(x.Id)).ToListAsync();
            role.Name = !string.IsNullOrEmpty(addRoleDto.Name) ? addRoleDto.Name : role.Name;
            role.Description = !string.IsNullOrEmpty(addRoleDto.Description) ? addRoleDto.Description : role.Description;
            //role.IsLoggedOutType = addRoleDto.IsLoggedOutType;
            role.IsLoggedOutType = !string.IsNullOrEmpty(addRoleDto.IsLoggedOutType.ToString()) ? addRoleDto.IsLoggedOutType ?? false : role.IsLoggedOutType;


            if (addRoleDto.LinkUsers?.Any() == true)
            {
                role.Users = allUsers;
            }

            _context.Roles.Update(role);

            return new GetRoleDto
            {
                Id = id,
                Name = role.Name,
                Description = role.Description,
                IsLoggedOutType = role.IsLoggedOutType ?? false,
                NoOfUsers = role.Users.Count
            };
        }
        public async Task<bool> DeleteAsync(object id)
        {
            try
            {
                ulong newId = (ulong)id;
                SQLCipher.Role role = await _context.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == newId);
                if (role == null)
                {
                    return false;
                }
                if (role.Users.Any())
                {
                    role.Users.Clear();
                }
                _context.Roles.Remove(role);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

