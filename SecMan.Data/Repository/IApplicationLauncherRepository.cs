using SecMan.Model;

namespace SecMan.Data.Repository
{
    public interface IApplicationLauncherRepository
    {
        Task<ApplicationLauncherResponse> GetInstalledApplicationsAsync();
    }
}
