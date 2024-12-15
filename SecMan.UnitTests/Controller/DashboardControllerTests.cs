using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;
using UserAccessManagement.Controllers;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardBL> _mockDashboardBAL;
        private readonly Mock<ILogger<DashboardController>> _mockDashboardControllerLogger;
        private readonly DashboardController _dashboardController;
        private readonly Mock<IMediator> _mockMediator;


        public DashboardControllerTests()
        {
            // Initialize mocks


            _mockDashboardControllerLogger = new Mock<ILogger<DashboardController>>();
            _mockDashboardBAL = new Mock<IDashboardBL>();
            _mockMediator = new Mock<IMediator>();
            _dashboardController = new DashboardController(_mockDashboardBAL.Object, _mockMediator.Object);
        }

        static DashboardControllerTests()
        {
        }

        // Positive Scenario 1: Data Exists
        [Fact]
        public async Task GetDashboard_ReturnsOkResult_WhenDataExists()
        {
            // Arrange: Set up your expected Dashboard data
            Dashboard expectedDashboard = new Dashboard
            {
                Zones = 5,
                Users = 10,
                Devices = 8,
                DevicesNotConfigured = 10,
                Roles = 10,
                RolesCreatedRecently = 10,
                UsersCreatedRecently = 10,
                ZonesCreatedRecently = 10
            };
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                             .ReturnsAsync(expectedDashboard);
            Log.Information("Completed Moqing dependencies");

            // Act: Call the controller method
            ActionResult<Dashboard> result = await _dashboardController.GetDashBoard();
            Log.Information("Test result: {@Result}", result);

            // Assert: Verify the result is OK and contains the expected data
            ActionResult<Dashboard> actionResult = Assert.IsType<ActionResult<Dashboard>>(result);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Log.Information("Verified if the response is of type OkObjectResult");
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Log.Information("Verified if the response objects status is OK:200");
            Assert.Equal(expectedDashboard, okResult.Value);
            Log.Information("Verified that mocked dashboard data is returned");
            _mockDashboardBAL.Verify(x => x.GetDashBoardResult(), Times.Once);
            Log.Information("Verified if the method return the proper Dashboard data");
        }

        // Positive Scenario 2: All Counts Are Zero
        [Fact]
        public async Task GetDashboard_ReturnsOkResult_WhenCountsAreZero()
        {
            // Arrange: Set up the dashboard data with zero counts
            Dashboard zeroDashboard = new Dashboard
            {
                Zones = 0,
                Users = 0,
                Devices = 0,
                DevicesNotConfigured = 0,
                Roles = 0,
                RolesCreatedRecently = 0,
                UsersCreatedRecently = 0,
                ZonesCreatedRecently = 0
            };
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                             .ReturnsAsync(zeroDashboard);
            Log.Information("Completed Moqing dependencies");

            // Act: Call the controller method
            ActionResult<Dashboard> result = await _dashboardController.GetDashBoard();
            Log.Information("Test result: {@Result}", result);

            // Assert: Verify that it returns Ok with the zero counts data
            ActionResult<Dashboard> actionResult = Assert.IsType<ActionResult<Dashboard>>(result);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Log.Information("Verified if the response is of type OkObjectResult");
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Log.Information("Verified if the response objects status is OK:200");
            Assert.Equal(zeroDashboard, okResult.Value);
            Log.Information("Verified that mocked dashboard data is returned");
            _mockDashboardBAL.Verify(x => x.GetDashBoardResult(), Times.Once);
            Log.Information("Verified if the method return the data with counts are zero");
        }

        // Negative Scenario 1: No Data Found (Null)
        [Fact]
        public async Task GetDashboard_ReturnsNotFound_WhenNoDataExists()
        {
            Dashboard? dashboard = null;

            // Arrange: Return null when no data exists
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                                 .ReturnsAsync(dashboard!);
            Log.Information("Completed Moqing dependencies");

            // Act: Call the controller method
            ActionResult<Dashboard> result = await _dashboardController.GetDashBoard();
            Log.Information("Test result: {@Result}", result);

            // Assert: Verify that it returns a NotFound (404)
            ActionResult<Dashboard> actionResult = Assert.IsType<ActionResult<Dashboard>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
            Log.Information("Verified if the response is of type NotFoundResult");
            _mockDashboardBAL.Verify(x => x.GetDashBoardResult(), Times.Once);
            Log.Information("Verified if the method return not found when data is not available");
        }

        // Negative Scenario 2: Database Error or Exception
        [Fact]
        public async Task GetDashboard_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange: Simulate an exception being thrown by the service layer
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                             .ThrowsAsync(new Exception("Database error"));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert Call the controller method
            await Assert.ThrowsAsync<Exception>(() => _dashboardController.GetDashBoard());
            Log.Information("Verified if the method return Internal Server Error");
        }

        // Negative Scenario 3: Empty Data Set
        [Fact]
        public async Task GetDashboard_ReturnsOkResult_WhenDataIsEmpty()
        {
            // Arrange: Return a valid but empty dashboard data set
            Dashboard emptyDashboard = new Dashboard();
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                             .ReturnsAsync(emptyDashboard);
            Log.Information("Completed Moqing dependencies");

            // Act: Call the controller method
            ActionResult<Dashboard> result = await _dashboardController.GetDashBoard();
            Log.Information("Test result: {@Result}", result);

            // Assert: Verify that it returns Ok with an empty dashboard object
            ActionResult<Dashboard> actionResult = Assert.IsType<ActionResult<Dashboard>>(result);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Log.Information("Verified if the response is of type OkObjectResult");
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Log.Information("Verified if the response objects status is Ok:200");
            Assert.Equal(emptyDashboard, okResult.Value);
            Log.Information("Verified that mocked dashboard data is returned");
            _mockDashboardBAL.Verify(x => x.GetDashBoardResult(), Times.Once);
            Log.Information("Verified if the method return empty data");
        }

        // Negative Scenario 4: Unauthorized Access
        [Fact]
        public async Task GetDashboard_ReturnsUnauthorized_WhenUserNotAuthorized()
        {
            // Arrange: Simulate that the user is not authorized (mock authorization service if needed)
            // Assuming that authorization logic exists in the controller and can return Unauthorized (this is just for example)
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                             .ThrowsAsync(new UnauthorizedAccessException("User not authorized"));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert Call the controller method
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _dashboardController.GetDashBoard());
            Log.Information("Verified if the method return Internal Server Error");
        }

        // Negative Scenario 5: Bad Request
        [Fact]
        public async Task GetDashboard_ReturnsBadRequest_WhenInvalidInputProvided()
        {
            // Arrange: Simulate an invalid input (if applicable, depending on how the method handles input validation)
            // Since the method doesn't take input, simulate a bad request scenario (e.g., invalid state or config)
            _mockDashboardBAL.Setup(bal => bal.GetDashBoardResult())
                             .ThrowsAsync(new ArgumentException("Invalid input"));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert Call the controller method
            await Assert.ThrowsAsync<ArgumentException>(() => _dashboardController.GetDashBoard());
            Log.Information("Verified if the method return Internal Server Error");
        }
    }
}
