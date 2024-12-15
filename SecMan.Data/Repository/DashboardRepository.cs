using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.Model.Common;
using Serilog;
using System.Data;

namespace SecMan.Data.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private Db _context { get; }
        public DashboardRepository(Db context)
        {
            _context = context;
        }
        public async Task<Dashboard> GetDashBoardResult(ulong? userId)
        {
            Log.Information("Started execution of {MethodName} in DAL", nameof(IDashboardRepository.GetDashBoardResult));

            Dashboard dashboardData = await _context.Zones
                                        .GroupBy(z => 1)
                                        .Select(g => new Dashboard
                                        {
                                            Zones = g.Count(),
                                            Users = _context.Users.Count(),
                                            Roles = _context.Roles.Count(),
                                            Devices = _context.Devs.Count()
                                        })
                                        .FirstOrDefaultAsync();

            Log.Information("Retrieved Zones count: {ZonesCount}", dashboardData.Zones);
            Log.Information("Retrieved Users count: {UsersCount}", dashboardData.Users);
            Log.Information("Retrieved Roles count: {RolesCount}", dashboardData.Roles);
            Log.Information("Retrieved Devices count: {DevicesCount}", dashboardData.Devices);

            SQLCipher.User user = await _context.Users
                        .Where(u => u.Id == userId)
                        .FirstOrDefaultAsync();

            EventLogs eventLog = await _context.EventLogs
                                          .Where(x => x.User == user &&
                                                      x.EventSubType == EventSubType.Login)
                                          .OrderByDescending(x => x.Date)
                                          .Skip(1)
                                          .Take(1)
                                          .FirstOrDefaultAsync();

            DateTime? lastLoginDate = eventLog?.Date ?? null;


            dashboardData.DevicesNotConfigured = await _context.Devs
                                                               .CountAsync(d => d.ConnState == SysFeatureConstants.DevicesConnStateIsZero);
            Log.Information("Retrieved Devices Not Configured count: {DevicesNotConfiguredCount}", dashboardData.DevicesNotConfigured);


            dashboardData.UsersCreatedRecently = await _context.Users
                                                    .CountAsync(u => u.CreatedDate > lastLoginDate &&
                                                                     u.CreatedDate <= DateTime.UtcNow);

            Log.Information("Retrieved password expiry count for users: {PasswordExpiryCount}", dashboardData.UsersCreatedRecently);


            dashboardData.ZonesCreatedRecently = await _context.Zones
                                                .Where(z => z.CreatedDate > lastLoginDate && z.CreatedDate <= DateTime.UtcNow)
                                                .CountAsync();

            Log.Information("Retrieved count of recent Zones created: {RecentZonesCount}", dashboardData.ZonesCreatedRecently);


            dashboardData.RolesCreatedRecently = await _context.Roles
                                                .Where(r => r.CreatedDate > lastLoginDate &&
                                                            r.CreatedDate <= DateTime.UtcNow)
                                                .CountAsync();

            Log.Information("Retrieved count of recent Roles created: {RecentRolesCount}", dashboardData.RolesCreatedRecently);

            Log.Information("Completed execution of {MethodName} in DAL", nameof(IDashboardRepository.GetDashBoardResult));

            return dashboardData;

        }
    }
}
