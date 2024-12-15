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
    public class SignatureRepository : GenericRepository<SQLCipher.EventLogs>, ISignatureRepository
    {
        private readonly Db _context;
        private readonly IHttpContextAccessor _httpContext;
        public SignatureRepository(Db context, IHttpContextAccessor httpContext) : base(context)
        {
            _context = context;
            _httpContext = httpContext;
        }
        public async Task<GetUserCredentialsDto> GetUserCredentials(string userName)
        {
                var existingUser = await _context.Users
                    .Where(x => x.UserName == userName)
                    .Select(x => new GetUserCredentialsDto
                    {
                        userId = x.Id,
                        Password = x.Password
                    })
                    .FirstOrDefaultAsync();
                return existingUser;
        }
        public async Task<bool> SignatureVerifyAsync(ulong userId, string note)
        {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var eventLogs = new EventLogs
                {
                    EventType = EventType.Signature,
                    EventSubType = EventSubType.Verify,
                    Severity = EventSeverity.Info,
                    User = user,
                    SigningUser = user,
                    Message = note,
                    Source = _httpContext?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Date = DateTime.UtcNow
                };
                _context.EventLogs.Add(eventLogs);
                return true;
        }
        public async Task SignatureAuthorizeAsync(ulong userId, ulong authorizeUserId, Authorize request)
        {
            var response = new ServiceResponse<Authorize>();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var authorizeUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == authorizeUserId);
            var eventLogs = new EventLogs
            {
                EventType = EventType.Signature,
                EventSubType = EventSubType.Authorize,
                Severity = EventSeverity.Info,
                User = user,
                SigningUser = user,
                AuthorizingUser = authorizeUser,
                Source = _httpContext?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                Date = DateTime.UtcNow
            };
            if (request.IsNote && !string.IsNullOrWhiteSpace(request.Note))
            {
                eventLogs.Message = request.Note;
            }
            _context.EventLogs.Add(eventLogs);
        }
    }
}
