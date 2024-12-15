using SecMan.Interfaces.DAL;
using SecMan.Model;

namespace SecMan.Data.DAL
{
    public interface IUserRepository : IGenericRepository<SQLCipher.User>
    {
        Task<Model.User> UpdateUserAsync(UpdateUser model, ulong userId, DateTime? passwordExpiryDate = null);

        Task<Model.User> AddUserAsync(CreateUser model);

        Task<SQLCipher.User?> GetUserByUsername(string username);

        Task<List<RoleModel>> GetRolesByRoleId(List<ulong> roleIds);

        Task<ulong> GetUserBySessionId(string sessionId);

        Task<Tuple<UserDetails?, List<AppPermissions>?>> GetUserDetails(ulong userId);

        Task UpdateUserSessionDetails(ulong userId, string sessionId, double sessionExpiryTime);

        Task RetireUserAsync(SQLCipher.User user);

        Task<bool> ClearUserSessionAsync(string sessionId);
        Task UnlockUserAsync(SQLCipher.User user, bool changePasswordOnLogin);
        Task<bool> UpdateUserLanguageAsync(ulong userId, string language);
    }
}
