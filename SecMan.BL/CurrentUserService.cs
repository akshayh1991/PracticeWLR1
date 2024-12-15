using Microsoft.AspNetCore.Http;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;

namespace SecMan.BL
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUnitOfWork _unitOfWork;

        public CurrentUserService(IHttpContextAccessor httpContext, IUnitOfWork unitOfWork)
        {
            _httpContext = httpContext;
            _unitOfWork = unitOfWork;
            if (_httpContext?.HttpContext.User.FindFirst(ResponseHeaders.SSOSessionId) != null)
            {
                SessionId = _httpContext?.HttpContext.User.FindFirst(ResponseHeaders.SSOSessionId).Value;
                ulong userId = _unitOfWork.IUserRepository.GetUserBySessionId(SessionId).Result;
                IsValidSession = userId != 0;
                if (IsValidSession)
                {
                    UserId = userId;
                    Tuple<UserDetails?, List<AppPermissions>?> userData = _unitOfWork.IUserRepository.GetUserDetails(userId).Result;
                    UserDetails = userData.Item1;
                    AppPermissions = userData.Item2;
                }
            }
        }

        public bool IsLoggedIn => SessionId != null;

        public string SessionId { get; init; }

        public bool IsValidSession { get; init; }

        public UserDetails? UserDetails { get; init; }

        public List<AppPermissions>? AppPermissions { get; init; }

        public ulong UserId { get; init; }
    }
}
