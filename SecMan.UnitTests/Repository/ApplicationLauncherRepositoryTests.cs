using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;

namespace SecMan.UnitTests.Repository
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class ApplicationLauncherRepositoryTests
    {
        private readonly DbContextOptions<Db> _dbContextOptions;
        private readonly Db _db;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly ApplicationLauncherRepository _repository;

        public ApplicationLauncherRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<Db>()
                    .UseSqlite("DataSource=:memory:")
                    .Options;

            _db = new Db(_dbContextOptions, string.Empty);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();

            IQueryable<Data.SQLCipher.DevDef> applications = new List<SecMan.Data.SQLCipher.DevDef>
                        {
                            new SecMan.Data.SQLCipher.DevDef { Id = 1, Name = "EPM", App = true },
                            new SecMan.Data.SQLCipher.DevDef { Id = 2, Name = "SecMan", App = true }
                        }.AsQueryable();
            _db.AddRange(applications);
            _db.SaveChanges();
            _mockConfig = new Mock<IConfiguration>();
            _repository = new ApplicationLauncherRepository(_db, _mockConfig.Object);
        }

        [Fact]
        public async Task GetInstalledApplicationsAsync_ReturnsApplicationsList_WhenDataIsAvailable()
        {
            // Arrange
            // Mock the configuration for version number
            _mockConfig.Setup(config => config["ApplicationLauncherSettings:Version"]).Returns("1.1");
            Log.Information("Completed Moqing dependencies");

            // Act
            ApplicationLauncherResponse result = await _repository.GetInstalledApplicationsAsync();
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.NotNull(result);
            Log.Information("Verified result: {@Result} is not null", result);
            Assert.Equal(1.1f, result.Version); // Check the version is as per the configuration
            Log.Information("Verified that mocked app version is returned");
            Assert.Equal(2, result.InstalledApps!.Count);
            Log.Information("Verified that mocked apps count matches with returned apps count");
        }

        [Fact]
        public async Task GetInstalledApplicationsAsync_ReturnsOk_WhenVersionIsMissing()
        {
            // Arrange
            _mockConfig.Setup(config => config["ApplicationLauncherSettings:Version"]).Returns<string>(null!);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApplicationLauncherResponse result = await _repository.GetInstalledApplicationsAsync();
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified result: {@Result} is not null", result);
            Assert.Equal(0f, result.Version);
            Log.Information("Verified that mocked app version is returned");
            Assert.Equal(2, result.InstalledApps!.Count);
            Log.Information("Verified that mocked apps count matches with returned apps count");
        }

        [Fact]
        public async Task GetInstalledApplicationsAsync_ReturnsEmptyList_WhenNoApplicationsInDb()
        {
            // Arrange
            _mockConfig.Setup(config => config["ApplicationLauncherSettings:Version"]).Returns("1.0");
            _db.RemoveRange(_db.DevDefs);
            _db.SaveChanges();
            Log.Information("Completed Moqing dependencies");

            // Act
            ApplicationLauncherResponse result = await _repository.GetInstalledApplicationsAsync();
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified result: {@Result} is not null", result);
            Assert.Empty(result.InstalledApps!);
            Log.Information("Verified that returned apps is not empty");
        }
    }
}


