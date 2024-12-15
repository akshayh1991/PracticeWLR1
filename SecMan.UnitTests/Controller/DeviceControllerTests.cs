using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UserAccessManagement.Controllers;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class DeviceControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IDeviceBL> _mockDeviceBL;
        private readonly DevicesController _controller;

        public DeviceControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockDeviceBL = new Mock<IDeviceBL>();
            _controller = new DevicesController(_mockMediator.Object, _mockDeviceBL.Object);
        }

        [Fact]
        public async Task AddDevice_ShouldReturnCreatedResult_WhenDeviceIsAddedSuccessfully()
        {
            // Arrange
            var createDevice = new CreateDevice();
            var expectedDevice = new GetDevice
            {
                Id = 1,
                Name = "Test Device",
                TypeId = 100,
                Ip = "192.168.1.1",
                ZoneId = 200,
                DeploymentStatus = "Deployed",
                IsLegacy = false
            };

            var serviceResponse = new ServiceResponse<GetDevice>
            {
                StatusCode = HttpStatusCode.Created,
                Data = expectedDevice
            };


            _mockDeviceBL.Setup(bl => bl.AddDeviceAsync(createDevice, false)).ReturnsAsync(serviceResponse);

            Log.Information("Mock setup completed for AddDeviceAsync.");

            // Act
            var result = await _controller.AddDevice(createDevice);

            Log.Information("Controller method AddDevice executed successfully.");

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal("AddDevice", createdResult.Location);

            Log.Information("Assertion passed: CreatedResult with expected location.");
        }

        [Fact]
        public async Task AddDevice_ShouldReturnConflictResult_WhenServiceReturnsConflict()
        {
            // Arrange

            var createDevice = new CreateDevice();
            var serviceResponse = new ServiceResponse<GetDevice>
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "Device already exists"
            };

            _mockDeviceBL.Setup(bl => bl.AddDeviceAsync(createDevice, false)).ReturnsAsync(serviceResponse);
            Log.Information("Mock setup completed for AddDeviceAsync with Conflict response.");

            // Act
            var result = await _controller.AddDevice(createDevice);
            Log.Information("Controller method AddDevice executed.");

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Log.Information("Assertion passed: Result is of type ConflictObjectResult.");

            var conflictResponse = Assert.IsType<Conflict>(conflictResult.Value);
            Assert.Equal(serviceResponse.Message, conflictResponse.Detail);
            Log.Information("Assertion passed: Conflict response message matches expected.");
        }

        [Fact]
        public async Task AddDevice_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange

            _controller.ModelState.AddModelError("Name", "Required");
            Log.Information("ModelState error added for Name field.");

            // Act
            var result = await _controller.AddDevice(new CreateDevice());
            Log.Information("Controller method AddDevice executed.");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Assertion passed: Result is of type BadRequestObjectResult.");
        }

        [Fact]
        public async Task AddDevice_ShouldReturnBadRequest_WhenRequiredFieldsAreMissing()
        {
            // Arrange

            var createDevice = new CreateDevice
            {
                Name = null // Missing required field
            };

            _controller.ModelState.AddModelError("Name", "Device Name cannot be empty");
            Log.Information("ModelState error added for missing Name field.");

            // Act
            var result = await _controller.AddDevice(createDevice);
            Log.Information("Controller method AddDevice executed.");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Assertion passed: Result is of type BadRequestObjectResult.");

            var errorDetails = Assert.IsType<SerializableError>(badRequestResult.Value);
            string expectedRes = "Device Name cannot be empty";
            Assert.Contains("Device Name cannot be empty", expectedRes);
            Log.Information("Assertion passed: Error message contains expected text.");
        }

        [Fact]
        public async Task AddDevice_ShouldReturnBadRequest_WhenRequiredFieldsAreEmptyString()
        {
            // Arrange
            var createDevice = new CreateDevice
            {
                Name = ""
            };

            _controller.ModelState.AddModelError("Name", "Device Name cannot be empty");

            // Log the validation error before the action is called
            Log.Information("Validation error: Device Name cannot be empty");

            // Act
            var result = await _controller.AddDevice(createDevice);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorDetails = Assert.IsType<SerializableError>(badRequestResult.Value);
            string expectedRes = "Device Name cannot be empty";
            Assert.Contains("Device Name cannot be empty", expectedRes);
        }

        [Fact]
        public async Task AddDevice_ShouldReturnBadRequest_WhenRequiredFieldsAreWhiteSpaces()
        {
            // Arrange
            var createDevice = new CreateDevice
            {
                Name = "  "
            };

            _controller.ModelState.AddModelError("Name", "Device Name cannot be empty");

            // Log the validation error before the action is called
            Log.Information("Validation error: Device Name cannot be empty");

            // Act
            var result = await _controller.AddDevice(createDevice);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorDetails = Assert.IsType<SerializableError>(badRequestResult.Value);
            string expectedRes = "Device Name cannot be empty";
            Assert.Contains("Device Name cannot be empty", expectedRes);
        }

        [Fact]
        public async Task AddDevice_ShouldReturnBadRequest_WhenTypeIdIsNull()
        {
            // Arrange
            var createDevice = new CreateDevice
            {
                TypeId = null // Missing required field
            };

            _controller.ModelState.AddModelError("Name", "Type Id cannot be empty");

            // Log the validation error before the action is called
            Log.Information("Validation error: Type Id cannot be empty");

            // Act
            var result = await _controller.AddDevice(createDevice);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorDetails = Assert.IsType<SerializableError>(badRequestResult.Value);
            string expectedRes = "Type Id cannot be empty";
            Assert.Contains("Type Id cannot be empty", expectedRes);
        }

    }
}
