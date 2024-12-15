using Microsoft.AspNetCore.Mvc.ModelBinding;
using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface IPasswordBl
    {
        Task<ServiceResponse<string>> UpdatePasswordAsync(string userName, string oldPassword, string newPassword, ModelStateDictionary modelState);
        Task<string> GenerateHashedToken(string userNamePassword);
        Task<ServiceResponse<GetForgetPasswordDto>> ForgetPasswordAsync(string userName);
        Task<ServiceResponse<bool>> GetUserNamePasswordAsync(string email, string token);
        Task<ServiceResponse<bool>> CheckForHashedToken(string token, string newPassword);
    }
}
