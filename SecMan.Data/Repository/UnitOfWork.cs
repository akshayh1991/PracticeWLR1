using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using SecMan.Data.DAL;
using SecMan.Data.Repository.IRepository;
using SecMan.Data.Repository.Repository;
using SecMan.Data.SQLCipher;

namespace SecMan.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Db _context;
        private readonly IHttpContextAccessor _httpContext;
        private IDbContextTransaction _transaction;
        private GenericRepository<User> _userRepository;
        private GenericRepository<SQLCipher.Role> _roleRepository;

        private RoleRepository _iRoleRepository;
        private DeviceRepository _iDeviceRepository;
        private UserRepository _iUserRepository;
        private SystemFeatureRepository _iSystemFeatureRepository;
        private SignatureRepository _iSignatureRepository;

        private DashboardRepository _iDashboardRepository;
        private ApplicationLauncherRepository _iApplicationLauncherRepository;
        private readonly IConfiguration _configuration;

        public UnitOfWork(Db context,
                         IHttpContextAccessor httpContext,
                         IConfiguration configuration)
        {
            _context = context;
            _httpContext = httpContext;
            _configuration = configuration;
        }

        public GenericRepository<User> UserRepository
        {
            get
            {
                return _userRepository ??= new GenericRepository<User>(_context);
            }
        }

        public GenericRepository<SQLCipher.Role> RoleRepository
        {
            get
            {
                return _roleRepository ??= new GenericRepository<SQLCipher.Role>(_context);
            }
        }
        public IRoleRepository IRoleRepository
        {
            get
            {
                return _iRoleRepository ??= new RoleRepository(_context);
            }
        }
        public IDeviceRepository IDeviceRepository
        {
            get
            {
                return _iDeviceRepository ??= new DeviceRepository(_context);
            }
        }

        public IDashboardRepository IDashboardRepository
        {
            get
            {
                return _iDashboardRepository ??= new DashboardRepository(_context);
            }
        }

        public IApplicationLauncherRepository IApplicationLauncherRepository
        {
            get
            {
                return _iApplicationLauncherRepository ??= new ApplicationLauncherRepository(_context, _configuration);
            }
        }

        public IUserRepository IUserRepository
        {
            get
            {
                return _iUserRepository ??= new UserRepository(_context);
            }
        }

        public IEventLogRepository IEventLogRepository
        {
            get
            {
                return new EventLogRepository(_context, _httpContext);
            }
        }

        public IPasswordRepository IPasswordRepository => new PasswordRepository(_context);

        public ISystemFeatureRepository ISystemFeatureRepository
        {
            get
            {
                return _iSystemFeatureRepository ??= new SystemFeatureRepository(_context);
            }
        }

        public ISignatureRepository ISignatureRepository
        {
            get
            {
                return _iSignatureRepository ??= new SignatureRepository(_context, _httpContext);
            }
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }


        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
}
