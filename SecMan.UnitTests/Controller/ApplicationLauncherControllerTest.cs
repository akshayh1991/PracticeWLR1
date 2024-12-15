using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using UserAccessManagement.Controllers;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class ApplicationLauncherControllerTest
    {
        private readonly Mock<IApplicationLauncherBL> _mockapplicationlauncherBL;
        private readonly Mock<ILogger<ApplicationLauncherController>> _mocklogger;
        private readonly ApplicationLauncherController _controller;
        private readonly Mock<IMediator> _mockMediator;
        public ApplicationLauncherControllerTest()
        {
            _mockapplicationlauncherBL = new Mock<IApplicationLauncherBL>();
            _mocklogger = new Mock<ILogger<ApplicationLauncherController>>();
            _mockMediator = new Mock<IMediator>();
            _controller = new ApplicationLauncherController(_mockapplicationlauncherBL.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task GetInstalledApplications_ReturnsOkWithResult_WhenDataExists()
        {
            // Arrange
            ApplicationLauncherResponse apps = new ApplicationLauncherResponse
            {
                Version = 1.1f,
                InstalledApps = new List<string>
                {
                    "EPM",
                    "SecMan",
                    "Reviewer",
                    "Strategy"
                }
            };
            _mockapplicationlauncherBL.Setup(x => x.GetInstalledApplicationsAsync())
                                                        .ReturnsAsync(apps);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult<ApplicationLauncherResponse> result = await _controller.GetInstalledApplications();
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Log.Information("Verified if the response is of type OkObjectResult");
            ApplicationLauncherResponse returnedApps = Assert.IsType<ApplicationLauncherResponse>(okResult.Value);
            Log.Information("Verified if the response object is of type ApplicationLauncherResponse");
            Assert.NotNull(returnedApps);
            Log.Information("Verified that returned Apps is not null");
            Assert.Equal(apps.Version, returnedApps.Version);
            Log.Information("Verified that mocked apps version is returned");
            Assert.Equal(apps.InstalledApps, returnedApps.InstalledApps);
            Log.Information("Verified that mocked apps are returned");
        }

        [Fact]
        public async Task GetInstalledApplications_ReturnsNotFound_WhenNoData()
        {
            // Arrange
            ApplicationLauncherResponse? applications = null;
            _mockapplicationlauncherBL.Setup(bl => bl.GetInstalledApplicationsAsync()).
                                                            ReturnsAsync(applications!);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult<ApplicationLauncherResponse> result = await _controller.GetInstalledApplications();
            Log.Information("Test result: {@Result}", result);

            // Assert
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
            Log.Information("Verified if the response is of type ObjectResult");
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Log.Information("Verified if the response status is NotFound:404");
        }

        [Fact]
        public async Task GetInstalledApplications_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockapplicationlauncherBL.Setup(bl => bl.GetInstalledApplicationsAsync()).ThrowsAsync(new Exception("Test exception"));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult<ApplicationLauncherResponse> result = await _controller.GetInstalledApplications();
            Log.Information("Test result: {@Result}", result);

            // Assert
            ObjectResult serverErrorResult = Assert.IsType<ObjectResult>(result.Result);
            Log.Information("Verified if the response is of type ObjectResult");
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
            Log.Information("Verified if the response status is InternalServerError:500");
        }

        [Fact]
        public async Task GetInstalledApplications_ReturnsNotFound_WhenInstalledAppsListIsEmpty()
        {
            // Arrange
            ApplicationLauncherResponse apps = new ApplicationLauncherResponse
            {
                Version = 1.1f,
                InstalledApps = new List<string>() //empty list passing
            };
            _mockapplicationlauncherBL.Setup(x => x.GetInstalledApplicationsAsync())
                .ReturnsAsync(apps);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult<ApplicationLauncherResponse> result = await _controller.GetInstalledApplications();
            Log.Information("Test result: {@Result}", result);

            // Assert
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result.Result);
            Log.Information("Verified if the response is of type ObjectResult");
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Log.Information("Verified if the response status is NotFound:404");
        }

        [Fact]
        public async Task GetInstalledApplications_ReturnsOk_WhenDataHasInvalidAppEntries()
        {
            // Arrange
            ApplicationLauncherResponse apps = new ApplicationLauncherResponse
            {
                Version = 1.1f,
                InstalledApps = new List<string>
                {
                    null!, // Invalid entry
                    "SecMan"
                }
            };
            _mockapplicationlauncherBL.Setup(x => x.GetInstalledApplicationsAsync())
                .ReturnsAsync(apps);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _controller.GetInstalledApplications();
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the response is of type OkObjectResult");
            ApplicationLauncherResponse returnedApps = Assert.IsType<ApplicationLauncherResponse>(okResult.Value);
            Log.Information("Verified if the response object is of type ApplicationLauncherResponse");
            Assert.NotNull(returnedApps);
            Log.Information("Verified that returned Apps is not null");
            Assert.Equal(returnedApps.InstalledApps!.Count, apps.InstalledApps.Count);
            Log.Information("Verified That Mocked Apps count is equal to returned apps");
        }


    }
}
