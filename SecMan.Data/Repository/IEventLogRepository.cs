using SecMan.Interfaces.DAL;
using SecMan.Model.Common;

namespace SecMan.Data.Repository
{
    public interface IEventLogRepository : IGenericRepository<SQLCipher.EventLogs>
    {
        Task LogLoginAttempts(ulong userId, bool isLoginSuccess, string useCase = EventSubType.Login);
    }
}
