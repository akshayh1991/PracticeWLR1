using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;
using UserAccessManagement.Controllers;
using UserAccessManagement.Handler;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class SignatureTests
    {
        private readonly Mock<ISignatureBL> _signatureBLMock;
        private readonly SignatureController _controller;
        private readonly Mock<IMediator> _mediatorMock;

        public SignatureTests()
        {
            _signatureBLMock = new Mock<ISignatureBL>();
            _mediatorMock = new Mock<IMediator>();
            _controller = new SignatureController(_signatureBLMock.Object, _mediatorMock.Object);
        }

        [Fact]
        public async Task VerifySignature_RequestIsNull_ReturnsBadRequest()
        {
            // Arrange
            VerifySignature request = null;

            // Act
            IActionResult result = await _controller.VerifySignature(request);
            Log.Information("Controller method executed: VerifySignature with null request, received result: {@Result}", result);

            // Assert
            ObjectResult? response = result as ObjectResult;
            Log.Information("Checking if the response is of type ObjectResult...");
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Log.Information("Verifying if StatusCode is 400...");
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Log.Information("Checking if the response body is of type BadRequest...");
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Log.Information("Verifying BadRequest details...");
            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.RequestEmpty, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: VerifySignature_RequestIsNull_ReturnsBadRequest");
        }

        [Fact]
        public async Task VerifySignature_SignatureVerificationFails_ReturnsBadRequest()
        {
            // Arrange
            VerifySignature request = new VerifySignature { Password = "test", Note = "test note" };

            _signatureBLMock
                .Setup(bl => bl.VerifySignatureAsync(request.Password, request.Note))
                .ReturnsAsync(new ApiResponse { StatusCode = HttpStatusCode.BadRequest });
            Log.Information("Mock setup completed for VerifySignatureAsync to return BadRequest");

            // Act
            IActionResult result = await _controller.VerifySignature(request);
            Log.Information("Controller method executed: VerifySignature with request: {@Request}, received result: {@Result}", request, result);

            // Assert
            ObjectResult? response = result as ObjectResult;
            Log.Information("Checking if the response is of type ObjectResult...");
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Log.Information("Verifying if StatusCode is 400...");
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Log.Information("Checking if the response body is of type BadRequest...");
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Log.Information("Verifying BadRequest details...");
            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.FailedSignatureVerified, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: VerifySignature_SignatureVerificationFails_ReturnsBadRequest");
        }

        [Fact]
        public async Task VerifySignature_SignatureVerificationSucceeds_ReturnsNoContent()
        {
            // Arrange
            VerifySignature request = new VerifySignature { Password = "test", Note = "test note" };

            _signatureBLMock
                .Setup(bl => bl.VerifySignatureAsync(request.Password, request.Note))
                .ReturnsAsync(new ApiResponse { StatusCode = HttpStatusCode.OK });
            Log.Information("Mock setup completed for VerifySignatureAsync to return OK");

            // Act
            IActionResult result = await _controller.VerifySignature(request);
            Log.Information("Controller method executed: VerifySignature with request: {@Request}, received result: {@Result}", request, result);

            // Assert
            NoContentResult? response = result as NoContentResult;
            Log.Information("Checking if the response is of type NoContentResult...");
            Assert.NotNull(response);
            Log.Information("Response is not null. Test successfully verified NoContentResult.");

            Log.Information("Test completed: VerifySignature_SignatureVerificationSucceeds_ReturnsNoContent");
        }

        [Fact]
        public async Task VerifySignature_LogsRequestAndResponse()
        {
            // Arrange
            VerifySignature request = new VerifySignature { Password = "test", Note = "test note" };

            ApiResponse response = new ApiResponse { StatusCode = HttpStatusCode.OK };
            _signatureBLMock
                .Setup(bl => bl.VerifySignatureAsync(request.Password, request.Note))
                .ReturnsAsync(response);
            Log.Information("Mock setup completed for VerifySignatureAsync with response: {@Response}", response);

            // Act
            await _controller.VerifySignature(request);
            Log.Information("Controller method executed: VerifySignature with request: {@Request}", request);

            // Assert
            Log.Information("Verifying that InfoLogCommand for request is logged...");
            _mediatorMock.Verify(m => m.Send(It.Is<InfoLogCommand>(cmd =>
                cmd.Message.Contains("VerifySignature method request") &&
                cmd.Properties[0] == request), default), Times.Once);
            Log.Information("Request log verification passed.");

            Log.Information("Verifying that InfoLogCommand for response is logged...");
            _mediatorMock.Verify(m => m.Send(It.Is<InfoLogCommand>(cmd =>
                cmd.Message.Contains("VerifySignature method response") &&
                cmd.Properties[0] == response), default), Times.Once);
            Log.Information("Response log verification passed.");

            Log.Information("Test completed: VerifySignature_LogsRequestAndResponse");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldAccept_WhenIsAuthorizeIsFalseEvenIfUserNameOrPasswordIsNotProvided()
        {
            // Arrange
            Authorize request = new Authorize
            {
                IsAuthorize = false,
                UserName = null,
                Password = null,
                IsNote = true,
                Note = "Test note",
                IsSigned = false
            };

            _signatureBLMock.Setup(x => x.SignatureAuthorizeAsync(It.IsAny<Authorize>()))
                            .ReturnsAsync(new ApiResponse(ResponseConstants.SignatureVerified, HttpStatusCode.OK));
            Log.Information("Mock setup completed for SignatureAuthorizeAsync with expected response: Signature Verified");

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            Log.Information("Checking if the result is of type NoContentResult...");
            Assert.IsType<NoContentResult>(result);
            Log.Information("Result verified as NoContentResult.");

            Log.Information("Test completed: AuthorizeSignature_ShouldAccept_WhenIsAuthorizeIsFalseEvenIfUserNameOrPasswordIsNotProvided");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnBadRequest_WhenAuthorizationFails()
        {
            // Arrange
            Authorize request = new Authorize
            {
                IsAuthorize = true,
                UserName = "user",
                Password = "wrongpassword",
                IsNote = true,
                Note = "Test note",
                IsSigned = false
            };

            _signatureBLMock.Setup(x => x.SignatureAuthorizeAsync(request))
                            .ReturnsAsync(new ApiResponse("Failed to authorize", HttpStatusCode.BadRequest));
            Log.Information("Mock setup completed for SignatureAuthorizeAsync to return BadRequest with response: Failed to authorize");

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            Log.Information("Checking if the response is of type ObjectResult...");
            ObjectResult? response = result as ObjectResult;
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Log.Information("Verifying if StatusCode is 400...");
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            Log.Information("Checking if the response body is of type BadRequest...");
            BadRequest? responseBody = response.Value as BadRequest;
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Log.Information("Verifying BadRequest details...");
            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal("Failed to authorize", responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnBadRequest_WhenAuthorizationFails");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnBadRequest_WhenIsSignedIsTrueAndUserNameOrPasswordIsMissing()
        {
            // Arrange
            Authorize request = new Authorize
            {
                IsAuthorize = true,
                IsSigned = true,
                UserName = null, // Username is missing
                Password = null  // Password is missing
            };

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            ObjectResult? response = result as ObjectResult;
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.IsAuthorizeUsernamePasswordRequired, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnBadRequest_WhenIsSignedIsTrueAndUserNameOrPasswordIsMissing");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnBadRequest_WhenIsNoteIsTrueAndNoteIsNullOrEmpty()
        {
            // Arrange
            Authorize request = new Authorize
            {
                IsNote = true,
                Note = "", // Note is empty
                IsAuthorize = false,
                IsSigned = false
            };

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            ObjectResult? response = result as ObjectResult;
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.NoteRequired, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnBadRequest_WhenIsNoteIsTrueAndNoteIsNullOrEmpty");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldAccept_WhenIsNoteIsFalseAndNoteIsNull()
        {
            // Arrange
            Authorize request = new Authorize
            {
                IsNote = false,
                Note = null,
                IsAuthorize = false,
                IsSigned = false
            };

            _signatureBLMock.Setup(x => x.SignatureAuthorizeAsync(It.IsAny<Authorize>()))
                            .ReturnsAsync(new ApiResponse(ResponseConstants.SignatureVerified, HttpStatusCode.OK));
            Log.Information("Mock setup completed for SignatureAuthorizeAsync with expected response: Signature Verified");

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            Assert.IsType<NoContentResult>(result); // Check that the result is NoContentResult
            Log.Information("Result verified as NoContentResult.");

            Log.Information("Test completed: AuthorizeSignature_ShouldAccept_WhenIsNoteIsFalseAndNoteIsNull");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            // Arrange
            Authorize request = null;

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with null request.");

            // Assert
            ObjectResult? response = result as ObjectResult;
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.RequestEmpty, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnBadRequest_WhenRequestIsNull");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnBadRequest_WhenNoteIsRequiredButEmpty()
        {
            // Arrange
            Authorize request = new Authorize { IsNote = true, Note = string.Empty };

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            ObjectResult? response = result as ObjectResult;
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.NoteRequired, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnBadRequest_WhenNoteIsRequiredButEmpty");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnBadRequest_WhenUsernameOrPasswordIsEmpty()
        {
            // Arrange
            Authorize request = new Authorize { IsAuthorize = true, UserName = string.Empty, Password = string.Empty };

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            ObjectResult? response = result as ObjectResult;
            Assert.NotNull(response);
            Log.Information("Response is not null. StatusCode: {StatusCode}", response?.StatusCode);

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Log.Information("StatusCode verified as 400 (Bad Request)");

            BadRequest? responseBody = response.Value as BadRequest;
            Assert.NotNull(responseBody);
            Log.Information("Response body is not null. Status: {Status}, Title: {Title}, Detail: {Detail}",
                responseBody?.Status, responseBody?.Title, responseBody?.Detail);

            Assert.Equal(HttpStatusCode.BadRequest, responseBody.Status);
            Assert.Equal(ResponseConstants.InvalidRequest, responseBody.Title);
            Assert.Equal(ResponseConstants.IsAuthorizeUsernamePasswordRequired, responseBody.Detail);
            Log.Information("BadRequest details verified successfully.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnBadRequest_WhenUsernameOrPasswordIsEmpty");
        }

        [Fact]
        public async Task AuthorizeSignature_ShouldReturnNoContent_WhenAuthorizationIsSuccessful()
        {
            // Arrange
            Authorize request = new Authorize
            {
                IsAuthorize = true,
                UserName = "testUser",
                Password = "testPassword"
            };

            _signatureBLMock.Setup(x => x.SignatureAuthorizeAsync(It.IsAny<Authorize>()))
                            .ReturnsAsync(new ApiResponse(ResponseConstants.SignatureVerified, HttpStatusCode.OK));
            Log.Information("Mock setup completed for SignatureAuthorizeAsync with expected response: Signature Verified");

            // Act
            ActionResult result = await _controller.AuthorizeSignature(request);
            Log.Information("Controller method executed: AuthorizeSignature with request: {@Request}", request);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Log.Information("Result verified as NoContentResult.");

            Log.Information("Test completed: AuthorizeSignature_ShouldReturnNoContent_WhenAuthorizationIsSuccessful");
        }
    }
}
