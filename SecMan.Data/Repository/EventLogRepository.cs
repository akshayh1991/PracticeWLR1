using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SecMan.BL.Common;
using SecMan.Data.DAL;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.Model.Common;
using Serilog;

namespace SecMan.Data.Repository
{
    partial class EventLogRepository : GenericRepository<SQLCipher.EventLogs>, IEventLogRepository
    {
        private readonly Db _context;
        private readonly IHttpContextAccessor _httpContext;

        public EventLogRepository(Db context, IHttpContextAccessor httpContext) : base(context)
        {
            _context = context;
            _httpContext = httpContext;
        }


        public async Task LogLoginAttempts(ulong userId, bool isLoginSuccess, string useCase = EventSubType.Login)
        {
            var countString = await _context.SysFeatProps.Where(x => x.Name == SysFeatureConstants.MaxLoginAttempts)
                                              .Select(x => x.Val)
                                              .FirstOrDefaultAsync();
            int.TryParse(countString, out var count);
            count = count == 0 ? 3 : count;
            SQLCipher.User user = await _context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();

            if (user is not null)
            {
                EventLogs loginLog = new EventLogs
                {
                    User = user,
                    EventType = EventType.User,
                    EventSubType = useCase,
                    Severity = EventSeverity.Info,
                    Message = useCase == EventSubType.Login ?
                                    (isLoginSuccess ? EventLogConstants.LoginSuccess : EventLogConstants.LoginFailed) :
                                    (isLoginSuccess ? EventLogConstants.LogoutSuccess : EventLogConstants.LogoutFailed),
                    Source = _httpContext?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                };
                await _context.AddAsync(loginLog);
                await _context.SaveChangesAsync();

                if (!isLoginSuccess && useCase == EventSubType.Login)
                {
                    List<EventLogs> userLogs = await _context.EventLogs
                                                 .Where(x => x.User == user &&
                                                             x.EventType == EventType.User &&
                                                             x.EventSubType == EventSubType.Login)
                                                 .OrderByDescending(x => x.Date)
                                                 .Take((int)count)
                                                 .ToListAsync();
                    Log.Information("User has made {@Count} failed login attempts", userLogs.Count);
                    if (userLogs.Count >= (int)count && userLogs.TrueForAll(x => x.Message == EventLogConstants.LoginFailed))
                    {
                        Log.Information("User is locked due to Exceeding Max Failed login Attempts");
                        user.Locked = true;
                        user.LockedDate = DateTime.UtcNow;
                        user.LockedReason = StringConstants.LockedByMultipleFailedLogin;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

    }
}
