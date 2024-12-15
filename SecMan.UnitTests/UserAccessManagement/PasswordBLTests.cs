using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Moq;
using SecMan.BL;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.Model.Common;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;

namespace SecMan.UnitTests.BL
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class PasswordBLTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEncryptionDecryption> _mockEncryptionDecryption;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ISendingEmail> _mockSendingEmail;
        private readonly PasswordBL _passwordBL;

        public PasswordBLTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEncryptionDecryption = new Mock<IEncryptionDecryption>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSendingEmail = new Mock<ISendingEmail>();
            _passwordBL = new PasswordBL(
                _mockUnitOfWork.Object,
                _mockEncryptionDecryption.Object,
                _mockConfiguration.Object,
                _mockSendingEmail.Object
            );
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnBadRequest_WhenOldPasswordAndNewPasswordAreSame()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            string userName = "testuser";
            string oldPassword = "SamePassword123!";
            string newPassword = "SamePassword123!";

            Log.Information("Test started: UpdatePasswordAsync_ShouldReturnBadRequest_WhenOldPasswordAndNewPasswordAreSame");

            // Act
            ServiceResponse<string> result = await _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword, modelState);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.True(modelState.ContainsKey("newPassword"));
            Assert.Equal(PasswordExceptionConstants.PasswordOldNewSameError, modelState["newPassword"].Errors[0].ErrorMessage);

            Log.Information("Test completed: UpdatePasswordAsync_ShouldReturnBadRequest_WhenOldPasswordAndNewPasswordAreSame");
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnBadRequest_WhenNewPasswordContainsUserName()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            string userName = "testuser";
            string oldPassword = "OldPassword123!";
            string newPassword = "testuser@2023";

            Log.Information("Test started: UpdatePasswordAsync_ShouldReturnBadRequest_WhenNewPasswordContainsUserName");

            // Act
            ServiceResponse<string> result = await _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword, modelState);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.True(modelState.ContainsKey("newPassword"));
            Assert.Equal(PasswordExceptionConstants.PasswordUserNameNewPasswordError, modelState["newPassword"].Errors[0].ErrorMessage);

            Log.Information("Test completed: UpdatePasswordAsync_ShouldReturnBadRequest_WhenNewPasswordContainsUserName");
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldReturnBadRequest_WhenPasswordDoesNotMeetComplexityRequirements()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            string userName = "testuser";
            string oldPassword = "OldPassword123!";
            string newPassword = "weak"; // Fails complexity
            GetPasswordComplexityDto complexityDto = new GetPasswordComplexityDto
            {
                minLength = 8,
                maxLength = 20,
                upperCase = 1,
                lowerCase = 1,
                numeric = 1,
                nonNumeric = 1
            };

            _mockUnitOfWork.Setup(u => u.IPasswordRepository.GetPasswordPropsFromSysFeatPropsAsync())
                .ReturnsAsync(complexityDto);

            _mockUnitOfWork.Setup(u => u.IPasswordRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(new UserCredentialsDto());

            Log.Information("Test started: UpdatePasswordAsync_ShouldReturnBadRequest_WhenPasswordDoesNotMeetComplexityRequirements");

            // Act
            ServiceResponse<string> result = await _passwordBL.UpdatePasswordAsync(userName, oldPassword, newPassword, modelState);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.True(modelState.ContainsKey("newPassword"));
            Assert.NotEmpty(modelState["newPassword"].Errors);

            Log.Information("Test completed: UpdatePasswordAsync_ShouldReturnBadRequest_WhenPasswordDoesNotMeetComplexityRequirements");
        }

        [Fact]
        public async Task ForgetPasswordAsync_ShouldReturnBadRequest_WhenUserNotFound()
        {
            // Arrange
            string userName = "nonexistentUser";
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.ForgetPasswordCredentials(userName))
                .ReturnsAsync((GetForgetPasswordDto)null);

            Log.Information("Test started: ForgetPasswordAsync_ShouldReturnBadRequest_WhenUserNotFound");

            // Act
            ServiceResponse<GetForgetPasswordDto> result = await _passwordBL.ForgetPasswordAsync(userName);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(PasswordExceptionConstants.PasswordUserNotFoundError, result.Message);

            Log.Information("Test completed: ForgetPasswordAsync_ShouldReturnBadRequest_WhenUserNotFound");
        }

        [Fact]
        public async Task ForgetPasswordAsync_ShouldReturnBadRequest_WhenTokenUpdateFails()
        {
            // Arrange
            string userName = "testUser";
            GetForgetPasswordDto userCredentials = new GetForgetPasswordDto
            {
                domain = "testDomain",
                userName = "testUser",
                userId = 10L,
                emailId = "test@example.com"
            };
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.ForgetPasswordCredentials(userName))
                .ReturnsAsync(userCredentials);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.UpdateHashedUserNamePassword(userCredentials.userId, It.IsAny<string>()))
                .ReturnsAsync((string)null);

            Log.Information("Test started: ForgetPasswordAsync_ShouldReturnBadRequest_WhenTokenUpdateFails");

            // Act
            ServiceResponse<GetForgetPasswordDto> result = await _passwordBL.ForgetPasswordAsync(userName);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(PasswordExceptionConstants.PasswordHashedTokenError, result.Message);

            Log.Information("Test completed: ForgetPasswordAsync_ShouldReturnBadRequest_WhenTokenUpdateFails");
        }

        [Fact]
        public async Task GenerateHashedToken_ShouldReturnEncryptedToken()
        {
            // Arrange
            string userNamePassword = "domain:userId:userName";
            string expectedHashedToken = "hashedToken123";

            Log.Information("Test started: GenerateHashedToken_ShouldReturnEncryptedToken");

            _mockEncryptionDecryption
                .Setup(x => x.EncryptPassword(userNamePassword, false))
                .Returns(expectedHashedToken);

            // Act
            string result = await _passwordBL.GenerateHashedToken(userNamePassword);

            // Assert
            Assert.Equal(expectedHashedToken, result);
            _mockEncryptionDecryption.Verify(x => x.EncryptPassword(userNamePassword, false), Times.Once);

            Log.Information("Test completed: GenerateHashedToken_ShouldReturnEncryptedToken");
        }

        [Fact]
        public async Task GetUserNamePasswordAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string email = "test@example.com";
            string token = "validToken";

            Log.Information("Test started: GetUserNamePasswordAsync_ShouldReturnNotFound_WhenUserDoesNotExist");

            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetUserNamePasswordFromEmailId(email.Trim()))
                           .ReturnsAsync((GetUserNamePasswordDto)null);

            // Act
            ServiceResponse<bool> result = await _passwordBL.GetUserNamePasswordAsync(email, token);

            // Assert
            Assert.False(result.Data);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(PasswordExceptionConstants.PasswordUserNotFoundError, result.Message);

            Log.Information("Test completed: GetUserNamePasswordAsync_ShouldReturnNotFound_WhenUserDoesNotExist");
        }

        [Fact]
        public async Task GetUserNamePasswordAsync_ShouldReturnUnauthorized_WhenTokenDoesNotMatch()
        {
            // Arrange
            string email = "test@example.com";
            string token = "invalidToken";
            GetUserNamePasswordDto userDto = new GetUserNamePasswordDto
            {
                hashedUserNamePassword = "validToken",
                hashedUserNamePasswordTime = DateTime.Now
            };

            Log.Information("Test started: GetUserNamePasswordAsync_ShouldReturnUnauthorized_WhenTokenDoesNotMatch");

            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetUserNamePasswordFromEmailId(email.Trim()))
                           .ReturnsAsync(userDto);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetPasswordExpiryWarningValue(SysFeatureConstants.ExpiryWarning))
                           .ReturnsAsync("01:00:00");

            // Act
            ServiceResponse<bool> result = await _passwordBL.GetUserNamePasswordAsync(email, token);

            // Assert
            Assert.False(result.Data);
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.Equal(PasswordExceptionConstants.PasswordTokenValidatedFailed, result.Message);

            Log.Information("Test completed: GetUserNamePasswordAsync_ShouldReturnUnauthorized_WhenTokenDoesNotMatch");
        }

        [Fact]
        public async Task GetUserNamePasswordAsync_ShouldReturnForbidden_WhenTokenHasExpired()
        {
            // Arrange
            string email = "test@example.com";
            string token = "validToken";
            GetUserNamePasswordDto userDto = new GetUserNamePasswordDto
            {
                hashedUserNamePassword = "validToken",
                hashedUserNamePasswordTime = DateTime.Now.AddHours(-2)
            };

            Log.Information("Test started: GetUserNamePasswordAsync_ShouldReturnForbidden_WhenTokenHasExpired");

            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetUserNamePasswordFromEmailId(email.Trim()))
                           .ReturnsAsync(userDto);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetPasswordExpiryWarningValue(SysFeatureConstants.ExpiryWarning))
                           .ReturnsAsync("01:00:00");

            // Act
            ServiceResponse<bool> result = await _passwordBL.GetUserNamePasswordAsync(email, token);

            // Assert
            Assert.False(result.Data);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Equal(PasswordExceptionConstants.PasswordTokenExpired, result.Message);

            Log.Information("Test completed: GetUserNamePasswordAsync_ShouldReturnForbidden_WhenTokenHasExpired");
        }

        [Fact]
        public async Task GetUserNamePasswordAsync_ShouldReturnSuccess_WhenTokenIsValid()
        {
            // Arrange
            string email = "test@example.com";
            string token = "validToken";
            GetUserNamePasswordDto userDto = new GetUserNamePasswordDto
            {
                hashedUserNamePassword = "validToken",
                hashedUserNamePasswordTime = DateTime.Now
            };

            Log.Information("Test started: GetUserNamePasswordAsync_ShouldReturnSuccess_WhenTokenIsValid");

            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetUserNamePasswordFromEmailId(email.Trim()))
                           .ReturnsAsync(userDto);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetPasswordExpiryWarningValue(SysFeatureConstants.ExpiryWarning))
                           .ReturnsAsync("01:00:00");

            // Act
            ServiceResponse<bool> result = await _passwordBL.GetUserNamePasswordAsync(email, token);

            // Assert
            Assert.True(result.Data);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(PasswordExceptionConstants.PasswordTokenValidatedSuccess, result.Message);

            Log.Information("Test completed: GetUserNamePasswordAsync_ShouldReturnSuccess_WhenTokenIsValid");
        }

    }
}
