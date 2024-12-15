using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface IRoleBL
    {
        Task<ServiceResponse<GetRoleDto>> AddRoleAsync(CreateRole addRoleDto, bool saveToDb = false);
        Task<IEnumerable<GetRoleDto>> GetAllRolesAsync();
        Task<ServiceResponse<GetRoleDto?>> GetRoleByIdAsync(ulong id);
        Task<ServiceResponse<GetRoleDto?>> UpdateRoleAsync(ulong id, UpdateRole addRoleDto, bool saveToDb = false);
        Task<ApiResponse> DeleteRoleAsync(ulong id, bool saveToDb = false);
        Task<ServiceResponse<string>> ValidateRoleNameAsync(string roleName);
        Task<ServiceResponse<string>> ExistingRoleName(string roleName);
        Task<ServiceResponse<string>> ExistingRoleNameWhileUpdation(string roleName, ulong id);
    }
}
