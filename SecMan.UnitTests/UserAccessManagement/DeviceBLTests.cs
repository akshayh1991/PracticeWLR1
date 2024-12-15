using Moq;
using SecMan.BL;
using SecMan.Data.Repository;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class DeviceBLTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPendingChangesManager> _mockPendingChangesManager;
        private readonly DeviceBL _deviceBL;

        public DeviceBLTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPendingChangesManager = new Mock<IPendingChangesManager>();
            _deviceBL = new DeviceBL(_mockUnitOfWork.Object, _mockPendingChangesManager.Object);
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldReturnConflict_WhenDeviceAlreadyExists()
        {
            // Arrange
            var createDevice = new CreateDevice { Name = "ExistingDevice", TypeId = 1, ZoneId = 1 };
            Log.Information("Arrange: Setting up mock for existing device name.");
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.GetDeviceByDevicename(createDevice.Name))
                           .ReturnsAsync(new Data.SQLCipher.Dev());

            // Act
            Log.Information("Act: Calling AddDeviceAsync with existing device name.");
            var result = await _deviceBL.AddDeviceAsync(createDevice);

            // Assert
            Log.Information("Assert: Verifying the response for existing device name.");
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified StatusCode is Conflict.");
            Assert.Equal(ResponseConstants.DeviceAlreadyExists, result.Message);
            Log.Information("Verified response message is 'DeviceAlreadyExists'.");
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldReturnBadRequest_WhenTypeIdDoesNotExist()
        {
            // Arrange
            var createDevice = new CreateDevice { Name = "NewDevice", TypeId = 1, ZoneId = 1 };
            Log.Information("Arrange: Setting up mock for invalid TypeId.");
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.GetDeviceByDevicename(createDevice.Name))
                           .ReturnsAsync((Data.SQLCipher.Dev)null);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsTypeIdExists(createDevice.TypeId))
                           .ReturnsAsync(false);

            // Act
            Log.Information("Act: Calling AddDeviceAsync with invalid TypeId.");
            var result = await _deviceBL.AddDeviceAsync(createDevice);

            // Assert
            Log.Information("Assert: Verifying the response for invalid TypeId.");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified StatusCode is BadRequest.");
            Assert.Equal(ResponseConstants.DeviceInvalidTypeId, result.Message);
            Log.Information("Verified response message is 'DeviceInvalidTypeId'.");
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldReturnBadRequest_WhenZoneIdDoesNotExist()
        {
            // Arrange
            var createDevice = new CreateDevice { Name = "NewDevice", TypeId = 1, ZoneId = 1 };
            Log.Information("Arrange: Setting up mock for invalid ZoneId.");
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.GetDeviceByDevicename(createDevice.Name))
                           .ReturnsAsync((Data.SQLCipher.Dev)null);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsTypeIdExists(createDevice.TypeId))
                           .ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsZoneIdExists(createDevice.ZoneId))
                           .ReturnsAsync(false);

            // Act
            Log.Information("Act: Calling AddDeviceAsync with invalid ZoneId.");
            var result = await _deviceBL.AddDeviceAsync(createDevice);

            // Assert
            Log.Information("Assert: Verifying the response for invalid ZoneId.");
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified StatusCode is BadRequest.");
            Assert.Equal(ResponseConstants.DeviceInvalidZoneId, result.Message);
            Log.Information("Verified response message is 'DeviceInvalidZoneId'.");
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldReturnSuccess_WhenSaveToDbIsTrue()
        {
            // Arrange
            var createDevice = new CreateDevice { Name = "NewDevice", TypeId = 1, ZoneId = 1 };
            var expectedDevice = new GetDevice { Id = 1, Name = "NewDevice" };
            Log.Information("Arrange: Setting up mocks for successful database save.");

            _mockUnitOfWork.Setup(u => u.IDeviceRepository.GetDeviceByDevicename(createDevice.Name))
                           .ReturnsAsync((Data.SQLCipher.Dev)null);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsTypeIdExists(createDevice.TypeId))
                           .ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsZoneIdExists(createDevice.ZoneId))
                           .ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.AddDeviceAsync(createDevice))
                           .ReturnsAsync(expectedDevice);

            // Act
            Log.Information("Act: Calling AddDeviceAsync with saveToDb set to true.");
            var result = await _deviceBL.AddDeviceAsync(createDevice, saveToDb: true);

            // Assert
            Log.Information("Assert: Verifying the response for successful device addition.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified StatusCode is OK.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified response message is 'Success'.");
            Assert.NotNull(result.Data);
            Log.Information("Verified result Data is not null.");
            Assert.Equal(expectedDevice.Id, result.Data.Id);
            Log.Information("Verified Data.Id matches expected device Id.");
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldReturnPendingChangesResponse_WhenSaveToDbIsFalse()
        {
            // Arrange
            var createDevice = new CreateDevice { Name = "NewDevice", TypeId = 1, ZoneId = 1 };
            var pendingChangesResponse = new ServiceResponse<string>("Added to session", HttpStatusCode.OK);
            Log.Information("Arrange: Setting up mocks for adding device to session.");

            _mockUnitOfWork.Setup(u => u.IDeviceRepository.GetDeviceByDevicename(createDevice.Name))
                           .ReturnsAsync((Data.SQLCipher.Dev)null);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsTypeIdExists(createDevice.TypeId))
                           .ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.IDeviceRepository.IsZoneIdExists(createDevice.ZoneId))
                           .ReturnsAsync(true);
            _mockPendingChangesManager.Setup(p => p.AddToSessionJsonAsync(createDevice, SecMan.BL.Common.JsonEntities.Device))
                                       .ReturnsAsync(pendingChangesResponse);

            // Act
            Log.Information("Act: Calling AddDeviceAsync with saveToDb set to false.");
            var result = await _deviceBL.AddDeviceAsync(createDevice, saveToDb: false);

            // Assert
            Log.Information("Assert: Verifying the response for adding device to session.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified StatusCode is OK.");
            Assert.Equal(pendingChangesResponse.Message, result.Message);
            Log.Information("Verified response message matches pendingChangesResponse.Message.");
        }
    }

}
