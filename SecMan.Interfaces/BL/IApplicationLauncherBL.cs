using SecMan.Model;


namespace SecMan.Interfaces.BL
{
    public interface IApplicationLauncherBL
    {
        Task<ApplicationLauncherResponse> GetInstalledApplicationsAsync();
    }
}
