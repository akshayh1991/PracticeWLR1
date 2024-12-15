using SecMan.Data.DAL;
using SecMan.Data.Repository.IRepository;

namespace SecMan.Data.Repository
{
    public interface IUnitOfWork
    {
        IRoleRepository IRoleRepository { get; }
        IDeviceRepository IDeviceRepository { get; }
        IUserRepository IUserRepository { get; }
        IPasswordRepository IPasswordRepository { get; }
        IDashboardRepository IDashboardRepository { get; }
        IEventLogRepository IEventLogRepository { get; }
        ISignatureRepository ISignatureRepository { get; }
        IApplicationLauncherRepository IApplicationLauncherRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        ISystemFeatureRepository ISystemFeatureRepository { get; }

    }
}
