using SecMan.Model;


namespace SecMan.Interfaces.BL
{
    public interface IAuthBL
    {
        Task<ServiceResponse<LoginServiceResponse>> LoginAsync(LoginRequest model);
        Task<bool> ClearUserSessionAsync(string sessionId);
        Task<ServiceResponse<LoginServiceResponse>> ValidateSessionAsync(string ssoSessionId);
    }
}
