using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using SecMan.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;
using UserAccessManagement.Controllers;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class ReviewChangesControllerTests
    {
        private readonly Mock<IReviewerBl> _mockReviewerBl;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ReviewChangesController _reviewChangesController;
        private readonly IFixture _fixture;

        public ReviewChangesControllerTests()
        {
            _mockReviewerBl = new Mock<IReviewerBl>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _reviewChangesController = new ReviewChangesController(_mockReviewerBl.Object, _mockHttpContextAccessor.Object);
            _fixture = new Fixture();

            _mockHttpContextAccessor.Setup(x => x.HttpContext.Request.Scheme)
                .Returns("http");
            _mockHttpContextAccessor.Setup(x => x.HttpContext.Request.Host)
                .Returns(new HostString("localhost"));
        }




        [Fact]
        public async Task GetUnsavedChanges_ReturnsOkResult_WithJObjectData()
        {
            // Arrange
            ServiceResponse<JObject> mockResponse = new SecMan.Model.ServiceResponse<JObject>
            {
                Data = new JObject
                {
                    ["key"] = "value"
                },
                Message = ResponseConstants.Success,
                StatusCode = HttpStatusCode.OK
            };
            _mockReviewerBl.Setup(x => x.ReadJsonData())
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _reviewChangesController.GetUnsavedChanges();
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the response is of type OkObjectResult");
            JObject returnValue = Assert.IsType<JObject>(okResult.Value);
            Log.Information("Verified if the response object is of type JObject");
            Assert.Equal("value", returnValue["key"].ToString());
            Log.Information("Verified if the response object value matchs mocked response");
        }

        [Fact]
        public async Task SaveChanges_ReturnsNoContent_WhenSaveIsSuccessful()
        {
            // Arrange
            JObject mockModel = new JObject
            {
                ["key"] = "value"
            };

            ApiResponse mockResponse = new SecMan.Model.ApiResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = ResponseConstants.Success
            };

            _mockReviewerBl.Setup(x => x.SaveUnsavedChanges(It.IsAny<JObject>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _reviewChangesController.SaveChanges(mockModel);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Log.Information("Verified if the response is of type NoContentResult");

        }


        [Fact]
        public async Task SaveChanges_ReturnsProblem_WhenInternalServerError()
        {
            // Arrange
            JObject mockModel = new JObject
            {
                ["key"] = "value"
            };

            ApiResponse mockResponse = new SecMan.Model.ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = ResponseConstants.NotFullyProcessed
            };

            _mockReviewerBl.Setup(x => x.SaveUnsavedChanges(It.IsAny<JObject>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _reviewChangesController.SaveChanges(mockModel);
            Log.Information("Test result: {@Result}", result);

            // Assert
            ObjectResult problemResult = Assert.IsType<ObjectResult>(result);
            Log.Information("Verified if the response is of type ObjectResult");
            Assert.Equal(500, problemResult.StatusCode);
            Log.Information("Verified if the response objects status is InternalServerError:500");
        }


        [Fact]
        public async Task SaveUnsavedJsonChanges_ReturnsNoContent_WhenSaveIsSuccessful()
        {
            // Arrange
            JObject mockModel = new JObject
            {
                ["key"] = "value"
            };
            ApiResponse mockResponse = new SecMan.Model.ApiResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = ResponseConstants.Success
            };
            _mockReviewerBl.Setup(x => x.SaveUnsavedJsonChanges(It.IsAny<JObject>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _reviewChangesController.SaveUnsavedJsonChanges(mockModel);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Log.Information("Verified if the response is of type NoContentResult");
        }
    }
}
