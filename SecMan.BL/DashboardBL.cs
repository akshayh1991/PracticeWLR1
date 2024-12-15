using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;


namespace SecMan.BL
{
    public class DashboardBL : IDashboardBL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DashboardBL(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves the dashboard result from the data access layer.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="Dashboard"/> object containing the dashboard data.
        /// </returns>
        public async Task<Dashboard> GetDashBoardResult()
        {
            ulong? userId = 0ul;
            if (_currentUserService.IsLoggedIn)
                userId = _currentUserService.UserDetails?.Id;
            return await _unitOfWork.IDashboardRepository.GetDashBoardResult(userId);
        }
    }
}
