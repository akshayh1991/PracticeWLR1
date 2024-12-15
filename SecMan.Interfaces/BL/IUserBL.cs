using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface IUserBL
    {
        Task<ServiceResponse<UsersWithCountDto>> GetUsersAsync(UsersFilterDto model);
        Task<ServiceResponse<User>> AddUserAsync(CreateUser model, bool saveToDb = false);
        Task<ServiceResponse<User>> GetUserByIdAsync(ulong userId);
        Task<ApiResponse> UnlockUserAsync(ulong userId, bool changePasswordOnLogin, bool saveToDb = false);
        Task<ServiceResponse<User>> UpdateUserAsync(UpdateUser model, ulong userId, bool saveToDb = false);
        Task<ApiResponse> DeleteUserAsync(ulong userId, bool saveToDb = false);
        Task<ApiResponse> RetireUserAsync(ulong userId, bool saveToDb = false);
        Task<ApiResponse> UpdateUserLanguageAsync(string language);
    }

}
