using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Moq;
using SecMan.BL.Common;
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
    public class UserControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IUserBL> _mockUserBl;
        private readonly UsersController _usersController;
        private readonly DefaultHttpContext _httpContext;
        private readonly Mock<IMediator> _mockMediator;


        public UserControllerTests()
        {
            _fixture = new Fixture();
            _mockUserBl = new Mock<IUserBL>();
            _mockMediator = new Mock<IMediator>();
            _usersController = new UsersController(_mockUserBl.Object, _mockMediator.Object);
            _httpContext = new DefaultHttpContext();
            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }


        // AddUserAsync
        [Fact]
        public async Task AddUserAsync_ShouldReturnConflict_WhenUsernameAlreadyExists()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.AddUserAsync(addUserDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Log.Information("Verified if the response is of type ConflictObjectResult");
            CommonResponse response = Assert.IsType<Conflict>(conflictResult.Value);
            Log.Information("Verified if the response object is of type Conflict");
            Assert.Equal(HttpStatusCode.Conflict, response.Status);
            Log.Information("Verified if the response objects status is Conflict:409");
            Assert.Equal(ResponseConstants.Conflict, response.Title);
            Log.Information("Verified if the response object's title is Conflict");
            _mockUserBl.Verify(x => x.AddUserAsync(addUserDto, false), Times.Once());
            Log.Information("Verified if the AddUserAsync method triggered only once");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnBadRequest_WhenUserNameAndPasswordAreSame()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserNameAndPasswordAreSame, HttpStatusCode.BadRequest));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.AddUserAsync(addUserDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the response is of type BadRequestObjectResult");
            BadRequest response = Assert.IsType<BadRequest>(badRequestResult.Value);
            Log.Information("Verified if the response object is of type BadRequest");
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Log.Information("Verified if the response objects status is BadRequest:400");
            Assert.Equal(ResponseConstants.UserNameAndPasswordAreSame, response.Detail);
            Log.Information("Verified if the response object's detail is UserNameAndPasswordAreSame");
            _mockUserBl.Verify(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the AddUserAsync method triggered only once");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnBadRequest_WhenInvalidRoleIdArePassed()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.AddUserAsync(addUserDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the response is of type BadRequestObjectResult");
            BadRequest response = Assert.IsType<BadRequest>(badRequestResult.Value);
            Log.Information("Verified if the response object is of type BadRequest");
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Log.Information("Verified if the response objects status is BadRequest:400");
            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, response.Detail);
            Log.Information("Verified if the response object's detail is SomeOfTheRoleNotPresent");
            _mockUserBl.Verify(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the AddUserAsync method triggered only once");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnBadRequest_WhenUserNameAlreadyPresent()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.AddUserAsync(addUserDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Log.Information("Verified if the response is of type BadRequestObjectResult");
            Conflict response = Assert.IsType<Conflict>(conflictResult.Value);
            Log.Information("Verified if the response object is of type BadRequest");
            Assert.Equal(HttpStatusCode.Conflict, response.Status);
            Log.Information("Verified if the response objects status is Conflict:409");
            Assert.Equal(ResponseConstants.UserAlreadyExists, response.Detail);
            Log.Information("Verified if the response object's detail is UserAlreadyExists");
            _mockUserBl.Verify(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the AddUserAsync method triggered only once");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnCreated_WhenUserIsCreated()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, user));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.AddUserAsync(addUserDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            CreatedResult createdResult = Assert.IsType<CreatedResult>(result);
            Log.Information("Verified if the response is of type CreatedResult");
            Assert.NotNull(createdResult.Value);
            Log.Information("Verified if the response object is not null");
            _mockUserBl.Verify(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the AddUserAsync method triggered only once");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _usersController.AddUserAsync(addUserDto));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // GetUsersAsync
        [Fact]
        public async Task GetUsersAsync_ShouldReturnOk_NoUsersExists()
        {
            // Arrange
            UsersFilterDto usersFilter = new UsersFilterDto
            {
                Limit = 1,
                Offset = 0,
                Username = "Test",
            };
            _mockUserBl.Setup(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()))
                .ReturnsAsync(new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = 0, Users = new List<User>() }));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.GetUsersAsync(usersFilter);
            Log.Information("Test result: {@Result}", result);

            // Arrange
            Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the response is of type OkObjectResult");
            _mockUserBl.Verify(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()), Times.Once());
            Log.Information("Verified if the GetUsersAsync method triggered only once");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnOk_WithListOfUsers()
        {
            // Arrange
            UsersFilterDto usersFilter = new UsersFilterDto
            {
                Limit = 10,
                Offset = 0,
                Username = "Test",
            };
            IEnumerable<User> users = _fixture.CreateMany<User>(10);
            _mockUserBl.Setup(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()))
                .ReturnsAsync(new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = users.Count(), Users = users.ToList() }));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.GetUsersAsync(usersFilter);
            Log.Information("Test result: {@Result}", result);

            // Arrange
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the response is of type OkObjectResult");
            List<User> returneredUsers = Assert.IsType<List<User>>(okResult.Value);
            Log.Information("Verified if the response object is of type List<User>");
            Assert.Equal(10, returneredUsers.Count);
            Log.Information("verified if the returnedUsers Count matches input filter value");
            Assert.Equal("10", _httpContext.Response.Headers[ResponseHeaders.TotalCount]);
            Log.Information("verified if the x-total-count response header had 10 as count");
            _mockUserBl.Verify(x => x.GetUsersAsync(usersFilter), Times.Once());
            Log.Information("Verified if the GetUsersAsync method triggered only once");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
        {
            // Arrange
            UsersFilterDto usersFilter = new UsersFilterDto
            {
                Limit = 10,
                Offset = 0,
                Username = "Test",
            };
            IEnumerable<User> users = _fixture.CreateMany<User>(10);
            _mockUserBl.Setup(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()))
                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _usersController.GetUsersAsync(usersFilter));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // GetUserByIdAsync
        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNotFound_WhenInvalidUserIdPassed()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.GetUserByIdAsync(It.IsAny<ulong>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.GetUserByIdAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NotFoundObjectResult notFoundResponse = Assert.IsType<NotFoundObjectResult>(result);
            Log.Information("Verified if the response is of type NotFoundObjectResult");
            NotFound responseData = Assert.IsType<NotFound>(notFoundResponse.Value);
            Log.Information("Verified if the response object is of type NotFound");
            Assert.Equal(ResponseConstants.UserDoesNotExists, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
            _mockUserBl.Verify(x => x.GetUserByIdAsync(It.IsAny<ulong>()), Times.Once());
            Log.Information("Verified if the GetUserByIdAsync method triggered only once");
        }


        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnOk_WithUserObject()
        {
            // Arrange
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.GetUserByIdAsync(It.IsAny<ulong>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, user));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.GetUserByIdAsync(user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResponse = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the response is of type OkObjectResult");
            User responseData = Assert.IsType<User>(okResponse.Value);
            Log.Information("Verified if the response is of type User");
            Assert.Equal(user, responseData);
            Log.Information("Verified if the mocked result object matches with result object");
            _mockUserBl.Verify(x => x.GetUserByIdAsync(user.Id), Times.Once());
            Log.Information("Verified if the GetUserByIdAsync method triggered only once");
        }


        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
        {
            // Arrange
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.GetUserByIdAsync(It.IsAny<ulong>()))
                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _usersController.GetUserByIdAsync(user.Id));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // UpdateUserAsync
        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNotFound_WhenInvalidUserIdIsPassed()
        {
            // Arrange
            UpdateUser user = _fixture.Create<UpdateUser>();
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.UpdateUserAsync(user, userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NotFoundObjectResult notFoundResponse = Assert.IsType<NotFoundObjectResult>(result);
            Log.Information("Verified if the response is of type NotFoundObjectResult");
            NotFound responseData = Assert.IsType<NotFound>(notFoundResponse.Value);
            Log.Information("Verified if the response object is of type NotFound");
            Assert.Equal(ResponseConstants.NotFound, responseData.Title);
            Log.Information("Verified if the response object's title is NotFound");
            Assert.Equal(ResponseConstants.UserDoesNotExists, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
            _mockUserBl.Verify(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenRetiredUserIdIsPassed()
        {
            // Arrange
            UpdateUser user = _fixture.Create<UpdateUser>();
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.CantEdit, HttpStatusCode.BadRequest));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.UpdateUserAsync(user, userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResponse = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the response is of type BadRequestObjectResult");
            BadRequest responseData = Assert.IsType<BadRequest>(badRequestResponse.Value);
            Log.Information("Verified if the response object is of type BadRequest");
            Assert.Equal(ResponseConstants.InvalidRequest, responseData.Title);
            Log.Information("Verified if the response object's title is InvalidRequest");
            Assert.Equal(ResponseConstants.CantEdit, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
            _mockUserBl.Verify(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenInvalidRoleIdIsPassed()
        {
            // Arrange
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.UpdateUserAsync(user, user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            BadRequestObjectResult badRequestResponse = Assert.IsType<BadRequestObjectResult>(result);
            Log.Information("Verified if the response is of type BadRequestObjectResult");
            BadRequest responseData = Assert.IsType<BadRequest>(badRequestResponse.Value);
            Log.Information("Verified if the response object is of type BadRequest");
            Assert.Equal(ResponseConstants.InvalidRequest, responseData.Title);
            Log.Information("Verified if the response object's title is InvalidRequest");
            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, responseData.Detail);
            Log.Information("Verified if the response object's Detail is SomeOfTheRoleNotPresent");
            _mockUserBl.Verify(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnConflict_WhenNameIsAlreadyPresent()
        {
            // Arrange
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.UpdateUserAsync(user, user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            ConflictObjectResult conflictResponse = Assert.IsType<ConflictObjectResult>(result);
            Log.Information("Verified if the response is of type ConflictObjectResult");
            Conflict responseData = Assert.IsType<Conflict>(conflictResponse.Value);
            Log.Information("Verified if the response object is of type Conflict");
            Assert.Equal(ResponseConstants.Conflict, responseData.Title);
            Log.Information("Verified if the response object's title is Conflict");
            Assert.Equal(ResponseConstants.UserAlreadyExists, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserAlreadyExists");
            _mockUserBl.Verify(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }



        [Fact]
        public async Task UpdateUserAsync_ShouldReturnForbid_WhenPermissionsAreNotPresent_ToUpdatePassword()
        {
            // Arrange
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.InvalidPermissions, HttpStatusCode.Forbidden));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.UpdateUserAsync(user, user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.IsType<ForbidResult>(result);
            Log.Information("Verified if the response is of type ForbidResult");
            _mockUserBl.Verify(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldReturnOk_WithUserObject()
        {
            // Arrange
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, user));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.UpdateUserAsync(user, user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            OkObjectResult okResponse = Assert.IsType<OkObjectResult>(result);
            Log.Information("Verified if the response is of type OkObjectResult");
            UpdatedResponse responseData = Assert.IsType<UpdatedResponse>(okResponse.Value);
            Log.Information("Verified if the response is of type User");
            Assert.Equal(user.Id, responseData.Id);
            Log.Information("Verified the mocked response object is equal to result object's value");
            _mockUserBl.Verify(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
        {
            User user = _fixture.Create<User>();
            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _usersController.UpdateUserAsync(user, user.Id));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // DeleteUserAsync
        [Fact]
        public async Task DeleteUserAsync_ShouldReturnNotFound_WhenInvalidUserIdIsPassed()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.DeleteUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NotFoundObjectResult notFoundResponse = Assert.IsType<NotFoundObjectResult>(result);
            Log.Information("Verified if the response is of type NotFoundObjectResult");
            NotFound responseData = Assert.IsType<NotFound>(notFoundResponse.Value);
            Log.Information("Verified if the response is of type NotFound");
            Assert.Equal(ResponseConstants.UserDoesNotExists, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
            _mockUserBl.Verify(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        [Fact]
        public async Task DeleteUserAsync_ShouldReturnNoContent_WhenUserIsDeleted()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");

            // Act
            ActionResult result = await _usersController.DeleteUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Log.Information("Verified if the response is of type NoContentResult");
            _mockUserBl.Verify(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        [Fact]
        public async Task DeleteUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                       .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");


            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _usersController.DeleteUserAsync(userId));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // RetireUserAsync
        [Fact]
        public async Task RetireUserAsync_ShouldReturnNotFound_WhenInvalidUserId_IsPassed()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.NotFound));
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _usersController.RetireUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            NotFoundObjectResult notFoundResponse = Assert.IsType<NotFoundObjectResult>(result);
            Log.Information("Verified if the response is of type NotFoundObjectResult");
            NotFound responseData = Assert.IsType<NotFound>(notFoundResponse.Value);
            Log.Information("Verified if the response is of type NotFound");
            Assert.Equal(ResponseConstants.UserDoesNotExists, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
            _mockUserBl.Verify(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        [Fact]
        public async Task RetireUserAsync_ShouldReturnConflict_WhenUser_IsAlreadyRetired()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.UserAlreadyRetired, HttpStatusCode.Conflict));
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _usersController.RetireUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            ConflictObjectResult conflictResponse = Assert.IsType<ConflictObjectResult>(result);
            Log.Information("Verified if the response is of type ConflictObjectResult");
            Conflict responseData = Assert.IsType<Conflict>(conflictResponse.Value);
            Log.Information("Verified if the response is of type Conflict");
            Assert.Equal(ResponseConstants.UserAlreadyRetired, responseData.Detail);
            Log.Information("Verified if the response object's Detail is UserAlreadyRetired");
            _mockUserBl.Verify(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        [Fact]
        public async Task RetireUserAsync_ShouldReturnNoContent_WhenSuccess()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUserBl.Setup(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");

            // Act
            IActionResult result = await _usersController.RetireUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Log.Information("Verified if the response is of type NoContentResult");
            _mockUserBl.Verify(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        // UnlockUserAsync
        [Fact]
        public async Task UnlockUserAsync_ReturnsNoContent_WhenUserIsUnlockedSuccessfully()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            bool changePasswordOnLogin = true;

            // Mock the business layer method
            _mockUserBl
                .Setup(bl => bl.UnlockUserAsync(userId, changePasswordOnLogin, It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));

            // Mock the mediator to simulate logging (optional)
            _mockMediator
                .Setup(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Unit());

            // Act
            IActionResult result = await _usersController.UnlockUserAsync(userId, changePasswordOnLogin);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
            _mockUserBl.Verify(bl => bl.UnlockUserAsync(userId, changePasswordOnLogin, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task UnlockUserAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            bool changePasswordOnLogin = true;

            // Mock the business layer method
            _mockUserBl
                .Setup(bl => bl.UnlockUserAsync(userId, changePasswordOnLogin, It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.NotFound, HttpStatusCode.NotFound));

            // Mock the mediator to simulate logging (optional)
            _mockMediator
                .Setup(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Unit());

            // Act
            IActionResult result = await _usersController.UnlockUserAsync(userId, changePasswordOnLogin);

            // Assert
            NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            NotFound notFoundResponse = Assert.IsType<NotFound>(notFoundResult.Value);
            Assert.Equal(ResponseConstants.NotFound, notFoundResponse.Detail);
            Assert.Equal(404, notFoundResult.StatusCode);
            _mockUserBl.Verify(bl => bl.UnlockUserAsync(userId, changePasswordOnLogin, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task UnlockUserAsync_SendsLoggingRequests()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            bool changePasswordOnLogin = true;

            // Mock the business layer method to return success
            _mockUserBl
                .Setup(bl => bl.UnlockUserAsync(userId, changePasswordOnLogin, It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));

            // Mock the mediator to simulate logging
            _mockMediator
                .Setup(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Unit());

            // Act
            await _usersController.UnlockUserAsync(userId, changePasswordOnLogin);

            // Assert
            _mockMediator.Verify(m => m.Send(It.Is<InfoLogCommand>(cmd => cmd.Message.Contains("UnlockUserAsync method request")), It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(m => m.Send(It.Is<InfoLogCommand>(cmd => cmd.Message.Contains("UnlockUserAsync method response")), It.IsAny<CancellationToken>()), Times.Once);
        }


        // LanguageCodeAsync
        [Fact]
        public async Task LanguageCodeAsync_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            Log.Information("Starting test: LanguageCodeAsync_ReturnsNoContent_WhenUpdateIsSuccessful1");

            string languageCode = nameof(Languages.en);
            ApiResponse response = new ApiResponse { StatusCode = HttpStatusCode.OK };

            // Log the input details
            Log.Information("Request data: {@LanguageCode}", languageCode);

            _mockUserBl.Setup(x => x.UpdateUserLanguageAsync(languageCode.ToString()))
                       .ReturnsAsync(response);

            // Act
            IActionResult result = await _usersController.LanguageCodeAsync(languageCode);

            // Log the result
            Log.Information("Result from LanguageCodeAsync: {@Result}", result);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Log.Information("Assertion passed: Result is NoContentResult.");

            // Verify the business layer method was called once with the correct language code
            _mockUserBl.Verify(x => x.UpdateUserLanguageAsync(It.Is<string>(code => code == languageCode.ToString())), Times.Once);
            Log.Information("Verified that UpdateUserLanguageAsync was called once with correct language code: {@LanguageCode}", languageCode);

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task LanguageCodeAsync_ReturnsBadRequest_WhenLanguageNotFound()
        {
            // Arrange
            string languageCode = nameof(Languages.fr);
            ApiResponse response = new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Language not found"
            };

            Log.Information("Starting test: LanguageCodeAsync_ReturnsBadRequest_WhenLanguageNotFound");

            _mockUserBl.Setup(x => x.UpdateUserLanguageAsync(languageCode.ToString()))
                       .ReturnsAsync(response);

            // Log the request
            Log.Information("LanguageCodeAsync method request: {@Request}", languageCode);

            // Act
            IActionResult result = await _usersController.LanguageCodeAsync(languageCode);

            // Log the result
            Log.Information("LanguageCodeAsync method response: {@Response}", response);

            // Assert
            NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, notFoundResult.StatusCode);

            // Log assertion passed for BadRequest
            Log.Information("Assertion passed: Response is BadRequestObjectResult with status code {StatusCode}.", notFoundResult.StatusCode);

            // Log success
            Log.Information("Test completed successfully.");
        }

    }
}
