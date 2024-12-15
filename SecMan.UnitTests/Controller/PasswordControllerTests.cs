using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
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
    public class PasswordControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IPasswordBl> _mockPasswordBl;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IAuthBL> _mockAuthBL;
        private readonly Mock<IMediator> _mockMediator;
        private readonly AuthController _authController;

        public PasswordControllerTests()
        {
            _fixture = new Fixture();
            _mockPasswordBl = new Mock<IPasswordBl>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockAuthBL = new Mock<IAuthBL>();
            _mockMediator = new Mock<IMediator>();
            _authController = new AuthController(
                _mockPasswordBl.Object,
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _mockAuthBL.Object,
                _mockMediator.Object
            );
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            Log.Information("Starting test: ChangePassword_ShouldReturnBadRequest_WhenModelStateIsInvalid");

            // Arrange
            ChangePasswordDto changePasswordDto = new ChangePasswordDto
            {
                userName = "testuser",
                oldPassword = "oldpassword",
                newPassword = "short" // Invalid password (less than 6 characters)
            };

            // Simulate invalid model state
            _authController.ModelState.AddModelError("newPassword", "The NewPassword field is invalid."); // Adding a specific error

            // Act
            IActionResult result = await _authController.ChangePassword(changePasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Check for ModelState errors
            SerializableError modelStateErrors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelStateErrors.ContainsKey("newPassword")); // Verify the error is for newPassword

            // Verify that UpdatePasswordAsync was not called
            _mockPasswordBl.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ModelStateDictionary>()), Times.Never());

            Log.Information("Test completed successfully: Model state is invalid as expected.");
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenChangePasswordDtoIsNull()
        {
            Log.Information("Starting test: ChangePassword_ShouldReturnBadRequest_WhenChangePasswordDtoIsNull");

            // Act
            IActionResult result = await _authController.ChangePassword(null);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request.", badRequestResult.Value);
            _mockPasswordBl.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ModelStateDictionary>()), Times.Never());

            Log.Information("Test completed successfully: ChangePasswordDto is null as expected.");
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            Log.Information("Starting test: ForgotPassword_ShouldReturnInternalServerError_WhenExceptionIsThrown");

            // Arrange
            ForgetPasswordDto forgetPasswordDto = new ForgetPasswordDto
            {
                userName = "testuser"
            };

            _mockPasswordBl.Setup(x => x.ForgetPasswordAsync(forgetPasswordDto.userName))
                .ThrowsAsync(new Exception("Some error occurred")); // Simulate an exception

            // Act
            IActionResult result = await _authController.ForgotPassword(forgetPasswordDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error. Please try again later.", statusCodeResult.Value);
            _mockPasswordBl.Verify(x => x.ForgetPasswordAsync(forgetPasswordDto.userName), Times.Once());

            Log.Information("Test completed successfully: Exception handled as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenResetPasswordDtoIsNull()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenResetPasswordDtoIsNull");

            // Arrange
            ResetPasswordDto resetPasswordDto = null;
            string authorizationHeader = "Bearer validToken";

            // Act
            ObjectResult? result = await _authController.ResetPassword(resetPasswordDto, authorizationHeader) as ObjectResult;
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

            ServiceResponse<bool> serviceResponse = Assert.IsType<ServiceResponse<bool>>(result.Value);
            Assert.Equal("Invalid request.", serviceResponse.Message);
            Assert.False(serviceResponse.Data);
            Assert.Equal("BadRequest", serviceResponse.StatusCode.ToString());

            Log.Information("Test completed successfully: ResetPasswordDto is null as expected.");
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsEmpty()
        {
            Log.Information("Starting test: ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsEmpty");

            // Arrange
            ResetPasswordDto resetPasswordDto = new ResetPasswordDto
            {
                newPassword = "" // Empty password
            };
            string authorizationHeader = "Bearer validToken";

            // Act
            ObjectResult? result = await _authController.ResetPassword(resetPasswordDto, authorizationHeader) as ObjectResult;
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

            ServiceResponse<bool> serviceResponse = Assert.IsType<ServiceResponse<bool>>(result.Value);
            Assert.Equal("New password is required.", serviceResponse.Message);
            Assert.False(serviceResponse.Data);
            Assert.Equal("BadRequest", serviceResponse.StatusCode.ToString());

            Log.Information("Test completed successfully: New password is empty as expected.");
        }


        [Fact]
        public async Task ResetPassword_ValidTokenAndEmail_ShouldRedirect()
        {
            // Arrange
            string token = "valid_token";
            string email = "test@example.com";

            // Mock the response from _passwordBL.GetUserNamePasswordAsync
            ServiceResponse<bool> response = new ServiceResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                Data = true
            };
            _mockConfiguration.Setup(x => x["ResetPassword:RedirectURL"])
                    .Returns("http://localhost/reset");
            _mockPasswordBl.Setup(bl => bl.GetUserNamePasswordAsync(email, token)).ReturnsAsync(response);

            // Set up the HTTP context with a valid bearer token
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer dummy_token";

            ControllerContext controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _authController.ControllerContext = controllerContext;

            // Dummy value for BaseURL
            string dummyBaseUrl = "http://localhost";

            // Log before the action
            Log.Information("Executing ResetPassword method with token: {Token}, email: {Email}", token, email);

            // Act
            RedirectResult? result = await _authController.ResetPassword(token, email) as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RedirectResult>(result);

            string expectedRedirectUrl = $"{dummyBaseUrl}/reset?token=dummy_token";
            Assert.Equal(expectedRedirectUrl, result.Url);

            // Log after the action
            Log.Information("ResetPassword method completed with redirect URL: {RedirectUrl}", result.Url);
        }


    }
}
