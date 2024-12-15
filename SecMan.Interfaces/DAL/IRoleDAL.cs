using SecMan.Model;

namespace SecMan.Interfaces.DAL
{
    public interface IRoleDal
    {
        Task<List<GetRoleDto>> GetAllRolesAsync();
    }
}
