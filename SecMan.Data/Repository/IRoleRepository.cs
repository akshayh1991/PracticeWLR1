using SecMan.Interfaces.DAL;
using SecMan.Model;

namespace SecMan.Data.DAL
{
    public interface IRoleRepository : IGenericRepository<SQLCipher.Role>
    {
        Task<SQLCipher.Role> AddRoleAsync(CreateRole addRoleDto);
        Task<GetRoleDto> UpdateRoleFromJsonAsync(ulong id, UpdateRole addRoleDto);
        Task<bool> IsRoleNameExistsAsync(ulong id, string name);
        Task<bool> ValidateLinkUsersAsync(List<ulong> linkUserIds);
        Task<bool> IsRoleNameExistsForCreationAsync(string name);
        Task<bool> DeleteAsync(object id);
        Task<bool> GetUserRetiredStatusAsync(ulong userId);
    }
}
