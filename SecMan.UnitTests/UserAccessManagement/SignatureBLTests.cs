using Moq;
using SecMan.BL;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class SignatureBLTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEncryptionDecryption> _encryptionDecryptionMock;
        private readonly Mock<ICurrentUserService> _currentUserServicesMock;
        private readonly SignatureBL _signatureBL;

        public SignatureBLTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _encryptionDecryptionMock = new Mock<IEncryptionDecryption>();
            _currentUserServicesMock = new Mock<ICurrentUserService>();
            _signatureBL = new SignatureBL(_unitOfWorkMock.Object, _encryptionDecryptionMock.Object, _currentUserServicesMock.Object);
        }

        [Fact]
        public async Task VerifySignatureAsync_UserCredentialsNotFound_ReturnsBadRequest()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync((GetUserCredentialsDto)null);

            Log.Information("Starting VerifySignatureAsync_UserCredentialsNotFound_ReturnsBadRequest test.");

            // Act
            ApiResponse result = await _signatureBL.VerifySignatureAsync("password", "note");
            Log.Information("Invoked VerifySignatureAsync with password and note.");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be BadRequest.");

            Assert.Equal(ResponseConstants.InvalidUsername, result.Message);
            Log.Information("Asserted result.Message to be InvalidUsername.");

            Log.Information("Finished VerifySignatureAsync_UserCredentialsNotFound_ReturnsBadRequest test with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
        }

        [Fact]
        public async Task VerifySignatureAsync_InvalidPasswordFormat_ReturnsBadRequest()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);

            GetUserCredentialsDto userCredentials = new GetUserCredentialsDto { Password = "invalidFormat" };
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(userCredentials);

            Log.Information("Starting VerifySignatureAsync_InvalidPasswordFormat_ReturnsBadRequest test.");

            // Act
            ApiResponse result = await _signatureBL.VerifySignatureAsync("password", "note");
            Log.Information("Invoked VerifySignatureAsync with password and note.");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be BadRequest.");

            Assert.Equal(ResponseConstants.InvalidPasswordFormat, result.Message);
            Log.Information("Asserted result.Message to be InvalidPasswordFormat.");

            Log.Information("Finished VerifySignatureAsync_InvalidPasswordFormat_ReturnsBadRequest test with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
        }

        [Fact]
        public async Task VerifySignatureAsync_UnsupportedPasswordFormat_ThrowsException()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);

            GetUserCredentialsDto userCredentials = new GetUserCredentialsDto { Password = "unsupported$3" };
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(userCredentials);

            Log.Information("Starting VerifySignatureAsync_UnsupportedPasswordFormat_ThrowsException test.");

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                _signatureBL.VerifySignatureAsync("password", "note"));
            Log.Information("Asserted that NotSupportedException is thrown.");

            Log.Information("Finished VerifySignatureAsync_UnsupportedPasswordFormat_ThrowsException test.");
        }

        [Fact]
        public async Task VerifySignatureAsync_ValidPassword_SHA256_VerifiesAndReturnsOk()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Id = 1, Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);

            GetUserCredentialsDto userCredentials = new GetUserCredentialsDto { Password = "hashed$2" };
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(userCredentials);

            _encryptionDecryptionMock.Setup(e => e.VerifyHashPassword(userCredentials.Password, "password"))
                .Returns(true);

            Log.Information("Starting VerifySignatureAsync_ValidPassword_SHA256_VerifiesAndReturnsOk test.");

            // Act
            ApiResponse result = await _signatureBL.VerifySignatureAsync("password", "note");
            Log.Information("Invoked VerifySignatureAsync with password and note.");

            // Assert
            _unitOfWorkMock.Verify(u => u.ISignatureRepository.SignatureVerifyAsync(userDetails.Id, "note"), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
            Log.Information("Asserted SignatureVerifyAsync and CommitTransactionAsync were called.");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be OK.");

            Assert.Equal(ResponseConstants.SignatureVerified, result.Message);
            Log.Information("Asserted result.Message to be SignatureVerified.");

            Log.Information("Finished VerifySignatureAsync_ValidPassword_SHA256_VerifiesAndReturnsOk test with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
        }

        [Fact]
        public async Task VerifySignatureAsync_InvalidPassword_SHA256_ReturnsBadRequest()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Id = 1, Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);

            GetUserCredentialsDto userCredentials = new GetUserCredentialsDto { Password = "hashed$2" };
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(userCredentials);

            _encryptionDecryptionMock.Setup(e => e.VerifyHashPassword(userCredentials.Password, "password"))
                .Returns(false);

            Log.Information("Starting VerifySignatureAsync_InvalidPassword_SHA256_ReturnsBadRequest test.");

            // Act
            ApiResponse result = await _signatureBL.VerifySignatureAsync("password", "note");
            Log.Information("Invoked VerifySignatureAsync with password and note.");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be BadRequest.");

            Assert.Equal(ResponseConstants.InvalidRequest, result.Message);
            Log.Information("Asserted result.Message to be InvalidRequest.");

            Log.Information("Finished VerifySignatureAsync_InvalidPassword_SHA256_ReturnsBadRequest test with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
        }

        [Fact]
        public async Task VerifySignatureAsync_ValidPassword_AES_VerifiesAndReturnsOk()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Id = 1, Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);

            GetUserCredentialsDto userCredentials = new GetUserCredentialsDto { Password = "encrypted$1" };
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(userCredentials);

            _encryptionDecryptionMock.Setup(e => e.DecryptPasswordAES256("encrypted$1"))
                .Returns("password"); // Simulate the decryption returning the correct password

            Log.Information("Starting VerifySignatureAsync_ValidPassword_AES_VerifiesAndReturnsOk test.");

            // Act
            ApiResponse result = await _signatureBL.VerifySignatureAsync("password", "note");
            Log.Information("Invoked VerifySignatureAsync with password and note.");

            // Assert
            _unitOfWorkMock.Verify(u => u.ISignatureRepository.SignatureVerifyAsync(userDetails.Id, "note"), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
            Log.Information("Asserted SignatureVerifyAsync and CommitTransactionAsync were called.");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be OK.");

            Assert.Equal(ResponseConstants.SignatureVerified, result.Message);
            Log.Information("Asserted result.Message to be SignatureVerified.");

            Log.Information("Finished VerifySignatureAsync_ValidPassword_AES_VerifiesAndReturnsOk test with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
        }

        [Fact]
        public async Task VerifySignatureAsync_InvalidPassword_AES_ReturnsBadRequest()
        {
            // Arrange
            UserDetails userDetails = new UserDetails { Id = 1, Username = "testUser" };
            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);

            GetUserCredentialsDto userCredentials = new GetUserCredentialsDto { Password = "encrypted$1" };
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync(userCredentials);

            _encryptionDecryptionMock.Setup(e => e.DecryptPasswordAES256("encrypted$1"))
                .Returns("wrongpassword"); // Simulate the decryption returning an incorrect password

            Log.Information("Starting VerifySignatureAsync_InvalidPassword_AES_ReturnsBadRequest test.");

            // Act
            ApiResponse result = await _signatureBL.VerifySignatureAsync("password", "note");
            Log.Information("Invoked VerifySignatureAsync with password and note.");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be BadRequest.");

            Assert.Equal(ResponseConstants.InvalidRequest, result.Message);
            Log.Information("Asserted result.Message to be InvalidRequest.");

            Log.Information("Finished VerifySignatureAsync_InvalidPassword_AES_ReturnsBadRequest test with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
        }

        [Fact]
        public async Task SignatureAuthorizeAsync_InvalidSignRequest_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            Authorize request = new Authorize { UserName = "nonExistentUser", IsSigned = true, IsAuthorize = false };
            UserDetails userDetails = new UserDetails { Id = 1, Username = "testUser" };

            Log.Information("Starting SignatureAuthorizeAsync_InvalidSignRequest_UserNotFound_ReturnsBadRequest test.");
            Log.Information("Request details: UserName = {UserName}, IsSigned = {IsSigned}, IsAuthorize = {IsAuthorize}",
                request.UserName, request.IsSigned, request.IsAuthorize);

            _currentUserServicesMock.Setup(s => s.UserDetails).Returns(userDetails);
            _unitOfWorkMock.Setup(u => u.ISignatureRepository.GetUserCredentials(It.IsAny<string>()))
                .ReturnsAsync((GetUserCredentialsDto)null);

            Log.Information("Setup complete: UserDetails mock returns UserName = {UserName}, UserId = {UserId}.",
                userDetails.Username, userDetails.Id);
            Log.Information("ISignatureRepository mock returns null for GetUserCredentials.");

            // Act
            ApiResponse result = await _signatureBL.SignatureAuthorizeAsync(request);
            Log.Information("Invoked SignatureAuthorizeAsync with request: {Request}.", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Asserted result.StatusCode to be BadRequest.");

            Assert.Equal(ResponseConstants.InvalidUsername, result.Message);
            Log.Information("Asserted result.Message to be InvalidUsername.");

            Log.Information("Finished SignatureAuthorizeAsync_InvalidSignRequest_UserNotFound_ReturnsBadRequest test with StatusCode: {StatusCode}, Message: {Message}",
                result.StatusCode, result.Message);
        }
    }
}

