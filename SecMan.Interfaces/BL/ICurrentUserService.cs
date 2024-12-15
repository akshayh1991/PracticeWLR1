using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface ICurrentUserService
    {
        List<AppPermissions>? AppPermissions { get; }
        bool IsLoggedIn { get; }
        UserDetails? UserDetails { get; }
        ulong UserId { get; }
        string SessionId { get; }
        bool IsValidSession { get; init; }
    }
}
