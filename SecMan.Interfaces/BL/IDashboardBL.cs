using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface IDashboardBL
    {
        Task<Dashboard> GetDashBoardResult();
    }
}
