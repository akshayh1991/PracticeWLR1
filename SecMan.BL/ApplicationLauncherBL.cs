using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;


namespace SecMan.BL
{
    public class ApplicationLauncherBL : IApplicationLauncherBL
    {
        private readonly IUnitOfWork _unitOfWork;
        public ApplicationLauncherBL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Asynchronously retrieves the list of installed applications from the repository.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains an <see cref="ApplicationLauncherResponse"/> 
        /// object that holds the version and list of installed applications.
        /// </returns>
        public async Task<ApplicationLauncherResponse> GetInstalledApplicationsAsync()
        {
            return await _unitOfWork.IApplicationLauncherRepository.GetInstalledApplicationsAsync();
        }
    }
}
