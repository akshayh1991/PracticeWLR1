using SecMan.Model;

namespace SecMan.Data.Repository
{
    public interface IDashboardRepository
    {
        Task<Dashboard> GetDashBoardResult(ulong? userId);
    }
}
