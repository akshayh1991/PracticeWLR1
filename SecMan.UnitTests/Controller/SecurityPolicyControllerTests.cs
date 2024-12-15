using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;
using UserAccessManagement.Controllers;
using Xunit.Abstractions;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class SecurityPolicyControllerTests
    {
        public readonly IFixture _fixture;
        public readonly Mock<ISystemFeatureBL> _mockSystemFeatureBL;
        private readonly Mock<IMediator> _mockMediator;
        public readonly SecurityPolicyController _securityPolicyController;

        public SecurityPolicyControllerTests(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _mockSystemFeatureBL = new Mock<ISystemFeatureBL>();
            _mockMediator = new Mock<IMediator>();
            _securityPolicyController = new SecurityPolicyController(_mockSystemFeatureBL.Object, _mockMediator.Object);
        }


        // GetSecurityPoliciesAsync
        [Fact]
        public async Task GetSecurityPolicyAsync_ShouldReturn_NoContent()
        {
            // Arrange
            ServiceResponse<List<SystemPolicies>> mockResponse = new ServiceResponse<List<SystemPolicies>>(ResponseConstants.Success, HttpStatusCode.OK);
            _mockSystemFeatureBL.Setup(x => x.GetSystemPoliciesAsync(It.IsAny<bool>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.GetSecurityPoliciesAsync();
            Log.Information("Test result: {@Result}", result);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Log.Information("Verified if the result is of NoContentResult");
        }


        [Fact]
        public async Task GetSecurityPolicyAsync_ShouldReturn_OkResult()
        {
            // Arrange
            List<SystemPolicies> systemPolicies = _fixture.CreateMany<SystemPolicies>(5).ToList();
            ServiceResponse<List<SystemPolicies>> mockResponse = new ServiceResponse<List<SystemPolicies>>(ResponseConstants.Success, HttpStatusCode.OK, systemPolicies);
            _mockSystemFeatureBL.Setup(x => x.GetSystemPoliciesAsync(It.IsAny<bool>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.GetSecurityPoliciesAsync();
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the result is of OkResponse");
            List<SystemPolicies> securityPolicyReturned = Assert.IsType<List<SystemPolicies>>(okResult.Value);
            Log.Information("Verified it the result body is of List<SystemPolicies> Type");
            Assert.Equal(systemPolicies, securityPolicyReturned);
            Log.Information("Verified if the mocked List<SystemPolicies> is returned from action method invocation");
        }



        // GetSecurityPolicyByIdAsync
        [Fact]
        public async Task GetSecurityPolicyByIdAsync_ShouldReturn_NotFound()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            ServiceResponse<List<SystemPolicyData>> mockResponse = new ServiceResponse<List<SystemPolicyData>>
                (ResponseConstants.InvalidId, HttpStatusCode.NotFound);
            _mockSystemFeatureBL.Setup(x => x.GetSystemPolicyByIdAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.GetSecurityPolicyByIdAsync(securityPolicyId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Log.Information("Verified if the result is of NotFoundObjectResult");
            NotFound responseObject = Assert.IsType<NotFound>(notFoundResult.Value);
            Log.Information("Verified it the result body is of NotFound Type");
            Assert.Equal(mockResponse.Message, responseObject.Detail);
            Log.Information("Verified if the response object's Detail is InvalidId");
        }



        [Fact]
        public async Task GetSecurityPolicyByIdAsync_ShouldReturn_OkResponse()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            List<SystemPolicyData> mocksecurityPolicy = _fixture.CreateMany<SystemPolicyData>(5).ToList();
            ServiceResponse<List<SystemPolicyData>> mockResponse = new ServiceResponse<List<SystemPolicyData>>
                (ResponseConstants.Success, HttpStatusCode.OK, mocksecurityPolicy);
            _mockSystemFeatureBL.Setup(x => x.GetSystemPolicyByIdAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.GetSecurityPolicyByIdAsync(securityPolicyId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the result is of OkObjectResult");
            List<SystemPolicyData> responseObject = Assert.IsType<List<SystemPolicyData>>(okResult.Value);
            Log.Information("Verified it the result body is of List<SystemPolicyData> Type");
            Assert.Equal(mocksecurityPolicy, responseObject);
            Log.Information("Verified if the response object is same as mocked object");
        }


        // UpdateSystemPolicyByIdAsync
        [Fact]
        public async Task UpdateSecurityPolicyByIdAsync_ShouldReturn_NotFound()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            List<UpdateSystemPolicyData> mockInput = _fixture.CreateMany<UpdateSystemPolicyData>(5).ToList();
            ServiceResponse<List<UpdatedResponse>> mockResponse = new ServiceResponse<List<UpdatedResponse>>
                (ResponseConstants.InvalidId, HttpStatusCode.NotFound);
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(),
                                                                          It.IsAny<List<UpdateSystemPolicyData>>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.UpdateSystemPolicyByIdAsync(securityPolicyId, mockInput);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Log.Information("Verified if the result is of NotFoundObjectResult");
            NotFound responseObject = Assert.IsType<NotFound>(notFoundResult.Value);
            Log.Information("Verified it the result body is of NotFound Type");
            Assert.Equal(mockResponse.Message, responseObject.Detail);
            Log.Information("Verified if the response object's Detail is InvalidId");
        }


        [Fact]
        public async Task UpdateSecurityPolicyByIdAsync_ShouldReturn_BadRequest_WhenInvalidEmailValueIsPassed()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            List<UpdateSystemPolicyData> mockInput = _fixture.CreateMany<UpdateSystemPolicyData>(5).ToList();
            ServiceResponse<List<UpdatedResponse>> mockResponse = new ServiceResponse<List<UpdatedResponse>>
                (ValidationConstants.InvalidEmail, HttpStatusCode.BadRequest);
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(),
                                                                          It.IsAny<List<UpdateSystemPolicyData>>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.UpdateSystemPolicyByIdAsync(securityPolicyId, mockInput);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the result is of BadRequestObjectResult");
            BadRequest responseObject = Assert.IsType<BadRequest>(badRequestResult.Value);
            Log.Information("Verified it the result body is of BadRequest Type");
            Assert.Equal(mockResponse.Message, responseObject.Detail);
            Log.Information("Verified if the response object's Detail is InvalidEmail");
        }


        [Fact]
        public async Task UpdateSecurityPolicyByIdAsync_ShouldReturn_BadRequest_WhenInvalidBoolValueIsPassed()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            List<UpdateSystemPolicyData> mockInput = _fixture.CreateMany<UpdateSystemPolicyData>(5).ToList();
            ServiceResponse<List<UpdatedResponse>> mockResponse = new ServiceResponse<List<UpdatedResponse>>
                (ValidationConstants.InvalidBoolean, HttpStatusCode.BadRequest);
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(),
                                                                          It.IsAny<List<UpdateSystemPolicyData>>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.UpdateSystemPolicyByIdAsync(securityPolicyId, mockInput);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the result is of BadRequestObjectResult");
            BadRequest responseObject = Assert.IsType<BadRequest>(badRequestResult.Value);
            Log.Information("Verified it the result body is of BadRequest Type");
            Assert.Equal(mockResponse.Message, responseObject.Detail);
            Log.Information("Verified if the response object's Detail is InvalidBoolean");
        }


        [Fact]
        public async Task UpdateSecurityPolicyByIdAsync_ShouldReturn_BadRequest_WhenInvalidIntValueIsPassed()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            List<UpdateSystemPolicyData> mockInput = _fixture.CreateMany<UpdateSystemPolicyData>(5).ToList();
            ServiceResponse<List<UpdatedResponse>> mockResponse = new ServiceResponse<List<UpdatedResponse>>
                (ValidationConstants.InvalidNumber, HttpStatusCode.BadRequest);
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(),
                                                                          It.IsAny<List<UpdateSystemPolicyData>>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.UpdateSystemPolicyByIdAsync(securityPolicyId, mockInput);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the result is of BadRequestObjectResult");
            BadRequest responseObject = Assert.IsType<BadRequest>(badRequestResult.Value);
            Log.Information("Verified it the result body is of BadRequest Type");
            Assert.Equal(mockResponse.Message, responseObject.Detail);
            Log.Information("Verified if the response object's Detail is InvalidNumber");
        }



        [Fact]
        public async Task UpdateSecurityPolicyByIdAsync_ShouldReturn_OkResponse()
        {
            // Arrange
            ulong securityPolicyId = _fixture.Create<ulong>();
            List<UpdateSystemPolicyData> mockInput = _fixture.CreateMany<UpdateSystemPolicyData>(5).ToList();
            ServiceResponse<List<UpdatedResponse>> mockResponse = new ServiceResponse<List<UpdatedResponse>>
                (ResponseConstants.Success, HttpStatusCode.OK, mockInput.Select(x => new UpdatedResponse { Id = x.Id }).ToList());
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(),
                                                                          It.IsAny<List<UpdateSystemPolicyData>>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<bool>(),
                                                                          It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(mockResponse);
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _securityPolicyController.UpdateSystemPolicyByIdAsync(securityPolicyId, mockInput);
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the result is of OkObjectResult");
            List<UpdatedResponse> responseObject = Assert.IsType<List<UpdatedResponse>>(okResult.Value);
            Log.Information("Verified it the result body is of List<UpdatedResponse> Type");
        }
    }
}
