using Microsoft.EntityFrameworkCore;
using Moq;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.UnitTests.Repository
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class DeviceRepositoryTests
    {
        private readonly DbContextOptions<Db> _dbContextOptions;
        private readonly Db _dbContext;
        private readonly DeviceRepository _deviceRepository;

        public DeviceRepositoryTests()
        {
            // Configure the in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<Db>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            // Provide a placeholder databaseFile argument
            string testDatabaseFile = "TestDbFile.db";

            // Initialize the Db context with the required parameters
            _dbContext = new Db(_dbContextOptions, testDatabaseFile);

            // Initialize the repository
            _deviceRepository = new DeviceRepository(_dbContext);
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldAddDeviceToDatabase_WhenValidDeviceIsProvided()
        {
            // Arrange

            // Clear existing data
            Log.Information("Clearing existing DevDefs and Zones from database.");
            _dbContext.DevDefs.RemoveRange(_dbContext.DevDefs);
            _dbContext.Zones.RemoveRange(_dbContext.Zones);
            await _dbContext.SaveChangesAsync();
            Log.Information("Cleared existing DevDefs and Zones.");

            var createDevice = new CreateDevice
            {
                Name = "Device1",
                TypeId = 1,
                ZoneId = 2,
                Ip = "192.168.1.1",
                DeploymentStatus = "Deployed",
                IsLegacy = false
            };

            var devDef = new DevDef { Id = 1, Name = "Type1" };
            var zone = new Zone { Id = 2, Name = "Zone1" };

            _dbContext.DevDefs.Add(devDef);
            _dbContext.Zones.Add(zone);
            await _dbContext.SaveChangesAsync();
            Log.Information("Added DevDef and Zone to the database.");

            // Act
            Log.Information("Calling AddDeviceAsync with CreateDevice model.");
            var result = await _deviceRepository.AddDeviceAsync(createDevice);

            // Assert
            Assert.NotNull(result);
            Log.Information("Asserted that result is not null.");

            Assert.Equal(createDevice.Name, result.Name);
            Log.Information("Asserted that device name matches the expected value.");

            Assert.Equal(createDevice.TypeId, result.TypeId);
            Log.Information("Asserted that device TypeId matches the expected value.");

            Assert.Equal(createDevice.ZoneId, result.ZoneId);
            Log.Information("Asserted that device ZoneId matches the expected value.");

            Log.Information("AddDeviceAsync test completed successfully with result: {@Result}", result);
        }


        [Fact]
        public async Task GetDeviceByDevicename_ShouldReturnDevice_WhenDeviceExists()
        {
            // Arrange

            var deviceName = "Device1";
            var device = new Dev { Id = 1, Name = deviceName };

            _dbContext.Devs.Add(device);
            await _dbContext.SaveChangesAsync();
            Log.Information("Added device with name '{DeviceName}' to the database.", deviceName);

            // Act
            Log.Information("Calling GetDeviceByDevicename with device name '{DeviceName}'.", deviceName);
            var result = await _deviceRepository.GetDeviceByDevicename(deviceName);

            // Assert
            Assert.NotNull(result);
            Log.Information("Asserted that result is not null.");

            Assert.Equal(deviceName, result.Name);
            Log.Information("Asserted that the retrieved device name matches the expected value.");

            Log.Information("GetDeviceByDevicename test completed successfully with result: {@Result}", result);
        }


        [Fact]
        public async Task GetDeviceByDevicename_ShouldReturnNull_WhenDeviceDoesNotExist()
        {
            // Arrange

            var deviceName = "NonExistentDevice";
            Log.Information("Device name set to '{DeviceName}', assuming it does not exist in the database.", deviceName);

            // Act
            Log.Information("Calling GetDeviceByDevicename with device name '{DeviceName}'.", deviceName);
            var result = await _deviceRepository.GetDeviceByDevicename(deviceName);

            // Assert
            Assert.Null(result);
            Log.Information("Asserted that the result is null, as expected for a non-existent device.");

            Log.Information("GetDeviceByDevicename_ShouldReturnNull test completed successfully.");
        }


        [Fact]
        public async Task IsTypeIdExists_ShouldReturnTrue_WhenTypeIdExists()
        {
            // Arrange

            var typeId = (ulong)1;
            var devDef = new DevDef { Id = typeId };

            _dbContext.DevDefs.Add(devDef);
            await _dbContext.SaveChangesAsync();
            Log.Information("Added DevDef with TypeId '{TypeId}' to the database.", typeId);

            // Act
            Log.Information("Calling IsTypeIdExists with TypeId '{TypeId}'.", typeId);
            var result = await _deviceRepository.IsTypeIdExists(typeId);

            // Assert
            Assert.True(result);
            Log.Information("Asserted that the result is true, indicating the TypeId exists in the database.");

            Log.Information("IsTypeIdExists_ShouldReturnTrue test completed successfully.");
        }


        [Fact]
        public async Task IsTypeIdExists_ShouldReturnFalse_WhenTypeIdDoesNotExist()
        {
            // Arrange

            var typeId = (ulong)999;
            Log.Information("TypeId set to '{TypeId}', which is expected not to exist in the database.", typeId);

            // Act
            Log.Information("Calling IsTypeIdExists with TypeId '{TypeId}'.", typeId);
            var result = await _deviceRepository.IsTypeIdExists(typeId);

            // Assert
            Assert.False(result);
            Log.Information("Asserted that the result is false, indicating the TypeId does not exist.");

            Log.Information("IsTypeIdExists_ShouldReturnFalse test completed successfully.");
        }


        [Fact]
        public async Task IsZoneIdExists_ShouldReturnTrue_WhenZoneIdExists()
        {
            // Arrange

            var zoneId = (ulong)2;
            var zone = new Zone { Id = zoneId };

            _dbContext.Zones.Add(zone);
            await _dbContext.SaveChangesAsync();
            Log.Information("Added Zone with ZoneId '{ZoneId}' to the database.", zoneId);

            // Act
            Log.Information("Calling IsZoneIdExists with ZoneId '{ZoneId}'.", zoneId);
            var result = await _deviceRepository.IsZoneIdExists(zoneId);

            // Assert
            Assert.True(result);
            Log.Information("Asserted that the result is true, indicating the ZoneId exists in the database.");

            Log.Information("IsZoneIdExists_ShouldReturnTrue test completed successfully.");
        }


        [Fact]
        public async Task IsZoneIdExists_ShouldReturnFalse_WhenZoneIdDoesNotExist()
        {
            // Arrange

            var zoneId = (ulong)999;
            Log.Information("ZoneId set to '{ZoneId}', which is expected not to exist in the database.", zoneId);

            // Act
            Log.Information("Calling IsZoneIdExists with ZoneId '{ZoneId}'.", zoneId);
            var result = await _deviceRepository.IsZoneIdExists(zoneId);

            // Assert
            Assert.False(result);
            Log.Information("Asserted that the result is false, indicating the ZoneId does not exist.");

            Log.Information("IsZoneIdExists_ShouldReturnFalse test completed successfully.");
        }

    }

}
