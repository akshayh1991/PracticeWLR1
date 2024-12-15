using SecMan.Interfaces.DAL;
using SecMan.Model;

namespace SecMan.Data.Repository
{
    public interface ISignatureRepository : IGenericRepository<SQLCipher.EventLogs>
    {
        Task<GetUserCredentialsDto> GetUserCredentials(string userName);
        Task<bool> SignatureVerifyAsync(ulong userId, string note);
        Task SignatureAuthorizeAsync(ulong userId, ulong authorizeUserId, Authorize request);
    }
}
