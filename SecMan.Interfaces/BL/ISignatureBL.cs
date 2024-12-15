using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface ISignatureBL
    {
        Task<ApiResponse> VerifySignatureAsync(string password, string note);
        Task<ApiResponse> SignatureAuthorizeAsync(Authorize request);
    }
}
