using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using SecMan.BL;
using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class UserTests : IDisposable
    {
        private readonly DbContextOptions<Db> _dbContextOptions;
        private readonly Db _db;
        private readonly IFixture _fixture;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<IEncryptionDecryption> _mockEncryptionDecryption;
        private readonly Mock<IRsaKeysBL> _mockRsaBl;
        private readonly Mock<IHttpContextAccessor> _mockHttpContext;
        public readonly Mock<IOptionsSnapshot<JwtTokenOptions>> _mockJwtTokenOptions;
        private UserBL _userBL;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IPendingChangesManager> _mockPendingChangesManager;

        public UserTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<Db>()
            .UseSqlite("DataSource=:memory:")
            .Options;

            _db = new Db(_dbContextOptions, string.Empty);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();
            List<Data.SQLCipher.Role> roles = new List<Data.SQLCipher.Role>()
                    {
                        new Data.SQLCipher.Role { Id = 1, Name = "Admin", Description = "Administrator role", IsLoggedOutType = false },
                        new Data.SQLCipher.Role { Id = 2, Name = "User", Description = "User role", IsLoggedOutType = false }
                    };
            _db.Roles.AddRange(roles);
            _db.Users.AddRange(
                    new Data.SQLCipher.User { Id = 1, UserName = "Admin", IsActive = true, Description = "Administrator role", Roles = new List<Data.SQLCipher.Role> { roles[0] }, IsLegacy = true },
                    new Data.SQLCipher.User { Id = 2, UserName = "User", Locked = true, Description = "User role", Roles = new List<Data.SQLCipher.Role> { roles[1] }, IsLegacy = false }
                );
            _db.SaveChanges();
            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockHttpContext = new Mock<IHttpContextAccessor>();
            _mockConfiguration = new Mock<IConfiguration>();

            _unitOfWork = new UnitOfWork(_db, _mockHttpContext.Object, _mockConfiguration.Object);
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEncryptionDecryption = new Mock<IEncryptionDecryption>();
            _mockRsaBl = new Mock<IRsaKeysBL>();
            _mockJwtTokenOptions = new Mock<IOptionsSnapshot<JwtTokenOptions>>();
            JwtTokenOptions jwtOpts = _fixture.Create<JwtTokenOptions>();
            _mockJwtTokenOptions.Setup(x => x.Value).Returns(jwtOpts);
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockPendingChangesManager = new Mock<IPendingChangesManager>();
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _mockUnitOfWork.Object,
                                 _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
        }


        // AddUserAsync
        [Fact]
        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
        {
            // Arrange
            CreateUser? addUserDto = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _userBL.AddUserAsync(addUserDto));
            Log.Information("Verified if the method throwing NullReferenceException for null input value");
        }


        [Fact]
        public async Task AddUserAsync_UserAlreadyExists_ReturnsConflict()
        {
            // Arrange
            CreateUser model = _fixture.Create<CreateUser>();
            Data.SQLCipher.User existingUser = _fixture.Create<SecMan.Data.SQLCipher.User>();

            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(model.Username))
                        .ReturnsAsync(existingUser);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(model);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified if the result object's status code is Conflict:409");
            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
            Log.Information("Verified if the result object's message is UserAlreadyExists");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(model.Username), Times.Once);
            Log.Information("Verified if the GetUserByUsername method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Never);
            Log.Information("Verified if the AddUserAsync method never triggered");
        }


        [Fact]
        public async Task AddUserAsync_UserAlreadyExists_ReturnsConflict_WhenSaveToDb_IsFalse()
        {
            // Arrange
            CreateUser model = _fixture.Create<CreateUser>();
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(model.Username))
                        .ReturnsAsync((Data.SQLCipher.User)null);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockPendingChangesManager.Setup(x => x.AddToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>()))
                        .ReturnsAsync(new ApiResponse(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict));
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(model);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified if the result object's status code is Conflict:409");
            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
            Log.Information("Verified if the result object's message is UserAlreadyExists");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(model.Username), Times.Once);
            Log.Information("Verified if the GetUserByUsername method triggered only once");
            _mockPendingChangesManager.Verify(dal => dal.AddToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>()), Times.Once);
            Log.Information("Verified if the AddToJsonAsync method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Never);
            Log.Information("Verified if the AddUserAsync method never triggered");
        }



        [Fact]
        public async Task AddUserAsync_UserNameAndPassword_Equal_ReturnsBadRequest()
        {
            // Arrange
            string userName = _fixture.Create<string>();
            CreateUser model = _fixture.Build<CreateUser>()
                .With(x => x.Username, userName)
                .With(x => x.Password, userName)
                .Create();
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(model);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified if the result object's status code is BadRequest:400");
            Assert.Equal(ResponseConstants.UserNameAndPasswordAreSame, result.Message);
            Log.Information("Verified if the result object's message is UserNameAndPasswordAreSame");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(model.Username), Times.Never);
            Log.Information("Verified if the GetUserByUsername method never triggered");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Never);
            Log.Information("Verified if the AddUserAsync method never triggered");
        }


        [Fact]
        public async Task AddUserAsync_InvalidRoles_ReturnsBadRequest()
        {
            // Arrange
            CreateUser model = _fixture.Create<CreateUser>();
            List<RoleModel> partialRoles = _fixture.CreateMany<RoleModel>(model.Roles.Count - 1).ToList();

            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(model.Username))
                        .ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(partialRoles);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(model);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified if the result object's status code is BadRequest:400");
            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, result.Message);
            Log.Information("Verified if the result object's message is SomeOfTheRoleNotPresent");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(model.Username), Times.Once);
            Log.Information("Verified if the GetUserByUsername method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Never);
            Log.Information("Verified if the AddUserAsync method never triggered");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenPasswordEncryption_ThrowsInvalidOperationException_WhenSaveDb_IsFalse()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            List<RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
            Model.User user = _fixture.Create<Model.User>();
            string encryptedPassword = _fixture.Create<string>();

            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                    .ReturnsAsync(roles);
            _mockEncryptionDecryption.Setup(enc => enc.EncryptPassword(It.IsAny<string>(), It.IsAny<bool>()))
                      .Throws<InvalidOperationException>();
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");


            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.AddUserAsync(addUserDto, false));
            Log.Information("Verified if the method throwing InvalidOperationException for any password encryption issues");
        }


        [Fact]
        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs_WhenSaveDb_IsTrue()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
            ulong userId = _fixture.Create<ulong>();
            string encryptedPassword = _fixture.Create<string>();

            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                    .ReturnsAsync(roles);
            _mockEncryptionDecryption.Setup(enc => enc.EncryptPassword(It.IsAny<string>(), It.IsAny<bool>()))
                      .Returns(encryptedPassword);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()))
                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");


            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.AddUserAsync(addUserDto, true));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }


        [Fact]
        public async Task AddUserAsync_SuccessfulCreation_ReturnsOk_WhenSaveDb_IsTrue()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
            Model.User user = _fixture.Create<Model.User>();
            string encryptedPassword = _fixture.Create<string>();

            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                    .ReturnsAsync(roles);
            _mockEncryptionDecryption.Setup(enc => enc.EncryptPassword(It.IsAny<string>(), It.IsAny<bool>()))
                      .Returns(encryptedPassword);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>())).ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(addUserDto, true);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            Assert.Equal(user.Id, result.Data?.Id);
            Log.Information("Verified of expected mocked object's : {@mockedObject} userid mathces with created objects : {@createdObject} userid", user, result);

            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username), Times.Once);
            Log.Information("Verified if the GetUserByUsername method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Once);
            Log.Information("Verified if the AddUserAsync method triggered only once");
        }


        [Fact]
        public async Task AddUserAsync_SuccessfulCreation_ReturnsOk_WhenSaveDb_IsFalse()
        {
            // Arrange
            CreateUser addUserDto = _fixture.Create<CreateUser>();
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                    .ReturnsAsync(roles);
            _mockPendingChangesManager.Setup(x => x.AddToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>()))
                        .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(addUserDto, false);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            Assert.Null(result.Data);
            Log.Information("Verified of expected response as null");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username), Times.Once);
            Log.Information("Verified if the GetUserByUsername method triggered only once");
            _mockPendingChangesManager.Verify(dal => dal.AddToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>()), Times.Once);
            Log.Information("Verified if the AddToJsonAsync method triggered only once");
        }




        // GetUsersAsync
        [Fact]
        public async Task GetUsersAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
        {
            // Arrange
            UsersFilterDto? filterDto = null;
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _unitOfWork, _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _userBL.GetUsersAsync(filterDto));
            Log.Information("Verified if the method throwing NullReferenceException for null input value");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccurs()
        {
            // Arrange
            UsersFilterDto filterDto = new UsersFilterDto
            {
                Limit = 1,
                Offset = 0,
                Username = "Admin",
            };
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetAll(r => r.Roles))
                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.GetUsersAsync(filterDto));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithUsernameExist()
        {
            // Arrange
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _unitOfWork, _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
            UsersFilterDto filterDto = new UsersFilterDto
            {
                Limit = 1,
                Offset = 0,
                Username = "Admin",
            };
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the result's data object which contains user objects is not null");
            Assert.Equal(1, result.Data.UserCount);
            Log.Information("Verified if the result contains one object based on filter and seeded data");
            Assert.Equal(filterDto.Limit, result.Data.Users.Count);
            Log.Information("Verified if the User Count Returned matches limit that has been queried");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithIsLegacyExist()
        {
            // Arrange
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _unitOfWork, _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
            UsersFilterDto filterDto = new UsersFilterDto
            {
                Limit = 10,
                Offset = 0,
                IsLegacy = true
            };
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the result's data object which contains user objects is not null");
            Assert.Equal(1, result.Data.UserCount);
            Log.Information("Verified if the result contains one object based on filter and seeded data");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithRoleExist()
        {
            // Arrange
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _unitOfWork, _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
            UsersFilterDto filterDto = new UsersFilterDto
            {
                Limit = 1,
                Offset = 0,
                Role = new List<string> { "Admin" },
                Status = []
            };
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the result's data object which contains user objects is not null");
            Assert.Equal(1, result.Data.UserCount);
            Log.Information("Verified if the result contains one object based on filter and seeded data");
            Assert.Equal(filterDto.Limit, result.Data.Users.Count);
            Log.Information("Verified if the User Count Returned matchs limit that has been queried");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithStatusExist()
        {
            // Arrange
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _unitOfWork, _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
            UsersFilterDto filterDto = new UsersFilterDto
            {
                Limit = 2,
                Offset = 0,
                Status = new List<string> { "active", "locked" },
                Role = []
            };
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the result's data object which contains user objects is not null");
            Assert.Equal(2, result.Data.UserCount);
            Log.Information("Verified if the result contains one object based on filter and seeded data");
            Assert.Equal(2, result.Data.Users.Count);
            Log.Information("Verified if the User Count Returned matchs limit that has been queried");
        }


        [Fact]
        public async Task GetUsersAsync_ShouldReturnNoUsers_WhenUsersWithFilterDoesNotExist()
        {
            // Arrange
            _userBL = new UserBL(_mockEncryptionDecryption.Object,
                                 _mockJwtTokenOptions.Object,
                                 _unitOfWork, _mockRsaBl.Object,
                                 _mockCurrentUserService.Object,
                                 _mockPendingChangesManager.Object);
            UsersFilterDto filterDto = new UsersFilterDto
            {
                Limit = 2,
                Offset = 0,
                Username = "test"
            };
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the result's data object which contains user objects is not null");
            Assert.Equal(0, result.Data.UserCount);
            Log.Information("Verified if the result contains 0 object based on filter and seeded data");
            Assert.Empty(result.Data.Users);
            Log.Information("Verified user object response is empty based on user filers");
        }



        // GetUserByIdAsync
        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserIdIsInvalid()
        {
            // Arrange
            ulong userId = 100ul;
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>()))
                .ReturnsAsync((Data.SQLCipher.User?)null);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.GetUserByIdAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the result's data object which contains user object is not null");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
            Log.Information("Verified if the result object's message is UserDoesNotExists");
        }


        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserIdIsValid()
        {
            // Arrange
            ulong userId = 1ul;
            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
            user.Id = userId;
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>(), r => r.Roles))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<Model.User> result = await _userBL.GetUserByIdAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the result's data object which contains user object is not null");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccurs()
        {
            // Arrange
            ulong userId = 1ul;
            Model.User user = _fixture.Create<Model.User>();
            user.Id = userId;
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>(), r => r.Roles))
                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.GetUserByIdAsync(userId));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // UpdateUserAsync
        [Fact]
        public async Task UpdateUserAsync_UserDoesNotExists_ReturnsBadRequest()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            ulong userId = 1ul;
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync((Data.SQLCipher.User?)null);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
            Log.Information("Verified if the result object's message is UserDoesNotExists");
            _mockUnitOfWork.Verify(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetSysFeatById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldReturnConflict_ForDuplicateUsername()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            ulong userId = 1ul;
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                                                .With(x => x.Retired, false).Create();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetAll(It.IsAny<Expression<Func<Data.SQLCipher.User, bool>>[]>()))
                        .ReturnsAsync(new List<Data.SQLCipher.User> { user });
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified if the result object's status code is Conflict:409");
            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
            Log.Information("Verified if the result object's message is UserAlreadyExists");
            _mockUnitOfWork.Verify(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetSysFeatById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }


        [Fact]
        public async Task UpdateUserAsync_CantEdit_IfUserIsRetired_ReturnsBadRequest()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            ulong userId = 1ul;
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                                                .With(x => x.Retired, true).Create();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified if the result object's status code is BadRequest:400");
            Assert.Equal(ResponseConstants.CantEdit, result.Message);
            Log.Information("Verified if the result object's message is CantEdit");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }



        [Fact]
        public async Task UpdateUserAsync_BadRequest_IfPasswordIsSameAsUsername()
        {
            // Arrange
            string username = _fixture.Create<string>();
            UpdateUser model = _fixture.Build<UpdateUser>()
                                       .With(x => x.Username, username)
                                       .With(x => x.Password, username)
                                       .Create();
            ulong userId = 1ul;
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                                               .With(x => x.Retired, false)
                                                .Create();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified if the result object's status code is BadRequest:400");
            Assert.Equal(ResponseConstants.UserNameAndPasswordAreSame, result.Message);
            Log.Information("Verified if the result object's message is UserNameAndPasswordAreSame");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
        {
            // Arrange
            UpdateUser? addUserDto = null;
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>()))
                .ReturnsAsync(user);
            ulong userId = 1ul;
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _userBL.UpdateUserAsync(addUserDto, userId));
            Log.Information("Verified if the method throwing NullReferenceException for null input value");
        }


        [Fact]
        public async Task UpdateUserAsync_InvalidRoles_ReturnsBadRequest()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            ulong userId = user.Id;
            List<Model.RoleModel> partialRoles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count - 1).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(partialRoles);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Log.Information("Verified if the result object's status code is BadRequest:400");
            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, result.Message);
            Log.Information("Verified if the result object's message is SomeOfTheRoleNotPresent");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }



        [Fact]
        public async Task UpdateUserAsync_InvalidPermissions_ReturnsBadRequest()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Build<Tuple<UserDetails?, List<AppPermissions>?>>()
                                      .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Log.Information("Verified if the result object's status code is Forbidden:403");
            Assert.Equal(ResponseConstants.InvalidPermissions, result.Message);
            Log.Information("Verified if the result object's message is InvalidPermissions");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }


        [Fact]
        public async Task UpdateUserAsync_Forbidden_WhenPermissionsAreNull()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            AppPermissions appPermissions = _fixture.Build<AppPermissions>()
                        .With(x => x.Permissions, (List<Permission>?)null)
                        .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = Tuple.Create<UserDetails?, List<AppPermissions>?>(
                _fixture.Create<UserDetails?>(),
                new List<AppPermissions> { appPermissions }
            );
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Log.Information("Verified if the result object's status code is Forbidden:403");
            Assert.Equal(ResponseConstants.InvalidPermissions, result.Message);
            Log.Information("Verified if the result object's message is InvalidPermissions");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }


        [Fact]
        public async Task UpdateUserAsync_Success_WhenPasswordIsUpdated()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            AppPermissions appPermissions = _fixture.Build<AppPermissions>()
                        .With(x => x.Permissions, _fixture.Build<Permission>()
                                                          .With(x => x.Name, "CAN_EDIT_ALL_PASSWORDS")
                                                          .CreateMany(2).ToList())
                        .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = Tuple.Create<UserDetails?, List<AppPermissions>?>(
                _fixture.Create<UserDetails?>(),
                new List<AppPermissions> { appPermissions }
            );
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetRecentPasswordsWithHistoryCountAsync(It.IsAny<ulong>()))
                .ReturnsAsync(new Tuple<List<string>, string>([], "3"));
            _mockPendingChangesManager.Setup(x => x.UpdateToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>(), It.IsAny<object>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is OK:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }



        [Fact]
        public async Task UpdateUserAsync_Success_WhenIsPasswordExpiryEnabled_IsNull()
        {
            // Arrange
            UpdateUser model = _fixture.Build<UpdateUser>()
                .With(x => x.IsPasswordExpiryEnabled, (bool?)null)
                .Create();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            AppPermissions appPermissions = _fixture.Build<AppPermissions>()
                        .With(x => x.Permissions, _fixture.Build<Permission>()
                                                          .With(x => x.Name, "CAN_EDIT_ALL_PASSWORDS")
                                                          .CreateMany(2).ToList())
                        .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = Tuple.Create<UserDetails?, List<AppPermissions>?>(
                _fixture.Create<UserDetails?>(),
                new List<AppPermissions> { appPermissions }
            );
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetRecentPasswordsWithHistoryCountAsync(It.IsAny<ulong>()))
                .ReturnsAsync(new Tuple<List<string>, string>([], "3"));
            _mockPendingChangesManager.Setup(x => x.UpdateToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>(), It.IsAny<object>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is OK:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }


        [Fact]
        public async Task UpdateUserAsync_Success_WhenIsPasswordExpiryEnabledIsTrue_AndValueInSysFeatIsInvalid()
        {
            // Arrange
            UpdateUser model = _fixture.Build<UpdateUser>()
                            .With(x => x.IsPasswordExpiryEnabled, true)
                            .Create();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Retired, false).Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "0")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            AppPermissions appPermissions = _fixture.Build<AppPermissions>()
                        .With(x => x.Permissions, _fixture.Build<Permission>()
                                                          .With(x => x.Name, "CAN_EDIT_ALL_PASSWORDS")
                                                          .CreateMany(2).ToList())
                        .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = Tuple.Create<UserDetails?, List<AppPermissions>?>(
                _fixture.Create<UserDetails?>(),
                new List<AppPermissions> { appPermissions }
            );
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetRecentPasswordsWithHistoryCountAsync(It.IsAny<ulong>()))
                .ReturnsAsync(new Tuple<List<string>, string>([], "3"));
            _mockPendingChangesManager.Setup(x => x.UpdateToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>(), It.IsAny<object>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is OK:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }



        [Fact]
        public async Task UpdateUserAsync_Success_ByChangingPassword_EncryptionType_WhenIsLegacyIsUpdated()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.IsLegacy, false)
                .With(x => x.Retired, false)
                .With(x => x.Password, (string?)null)
                .Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Build<Tuple<UserDetails?, List<AppPermissions>?>>()
                                      .Create();
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(model.Password);
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Log.Information("Verified if the result object's status code is Forbidden:409");
            Assert.Equal(ResponseConstants.InvalidPermissions, result.Message);
            Log.Information("Verified if the result object's message is InvalidPermissions");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime>()), Times.Never);
            Log.Information("Verified if the UpdateUserAsync method never triggered");
        }



        [Fact]
        public async Task UpdateUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs_WhenSaveToDbIsTrue()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.IsLegacy, false)
                .With(x => x.Retired, false)
                .With(x => x.Password, (string?)null)
                .Create();
            ulong userId = user.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            AppPermissions appPermissions = _fixture.Build<AppPermissions>()
                        .With(x => x.Permissions, _fixture.Build<Permission>()
                                                          .With(x => x.Name, "CAN_EDIT_ALL_PASSWORDS")
                                                          .CreateMany(2).ToList())
                        .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = Tuple.Create<UserDetails?, List<AppPermissions>?>(
                _fixture.Create<UserDetails?>(),
                new List<AppPermissions> { appPermissions }
            );
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(model.Password);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime?>()))
                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");


            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.UpdateUserAsync(model, userId, true));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }


        [Fact]
        public async Task UpdateUserAsync_SuccessfulCreation_ReturnsOk_WhenSaveToDbIsTrue()
        {
            // Arrange
            UpdateUser model = _fixture.Create<UpdateUser>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.IsLegacy, false)
                .With(x => x.Retired, false)
                .With(x => x.Password, (string?)null)
                .Create();
            Model.User addUserDto = _fixture.Create<Model.User>();
            addUserDto.Id = user.Id;
            ulong userId = addUserDto.Id;
            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count).ToList();
            SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                .With(x => x.Name, "Expiry")
                .With(x => x.Val, "30")
                .Create();
            Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
                .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
                .Create();
            AppPermissions appPermissions = _fixture.Build<AppPermissions>()
                        .With(x => x.Permissions, _fixture.Build<Permission>()
                                                          .With(x => x.Name, "CAN_EDIT_ALL_PASSWORDS")
                                                          .CreateMany(2).ToList())
                        .Create();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = Tuple.Create<UserDetails?, List<AppPermissions>?>(
                _fixture.Create<UserDetails?>(),
                new List<AppPermissions> { appPermissions }
            );
            _mockUnitOfWork.Setup(dal => dal.ISystemFeatureRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.SysFeat, object>>[]>()))
                        .ReturnsAsync(sysFeat);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                        .ReturnsAsync(user);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
                        .ReturnsAsync(roles);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                        .ReturnsAsync(userDetails);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(model.Password);
            _mockUnitOfWork.Setup(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime?>()))
                    .ReturnsAsync(addUserDto);
            _mockUnitOfWork.Setup(x => x.IPasswordRepository.GetRecentPasswordsWithHistoryCountAsync(It.IsAny<ulong>()))
                .ReturnsAsync(new Tuple<List<string>, string>([], "3"));
            _mockPendingChangesManager.Setup(x => x.UpdateToSessionJsonAsync(It.IsAny<object>(), It.IsAny<JsonEntities>(), It.IsAny<object>(), It.IsAny<ulong>(), It.IsAny<string>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            Log.Information("Completed Moqing dependencies");


            // Act
            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(addUserDto, userId, true);
            Log.Information("Test result: {@Result}", result);


            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            Assert.Equal(userId, result?.Data?.Id);
            Log.Information("Verified if the user id passed and updated user object's id are same({@userid}={@updatedUserId})", userId, result?.Data?.Id);
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            Log.Information("Verified if the GetUserById method triggered only once");
            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<DateTime?>()), Times.Once);
            Log.Information("Verified if the UpdateUserAsync method triggered only once");
        }



        // DeleteUserAsync
        [Fact]
        public async Task DeleteUserAsync_UserDoesNotExists_ReturnsNotFound()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>()))
                    .ReturnsAsync((Data.SQLCipher.User?)null);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.DeleteUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
            Log.Information("Verified if the result object's message is UserDoesNotExists");
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            _mockUnitOfWork.Verify(x => x.IUserRepository.Delete(It.IsAny<object>()), Times.Never);
            Log.Information("Verified if the DeleteUserAsync method never triggered");
        }


        [Fact]
        public async Task DeleteUserAsync_Successful_ReturnOk_WhenSaveToDbIsTrue()
        {
            // Arrange
            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(x => x.IUserRepository.Delete(It.IsAny<object>()))
                .ReturnsAsync(true);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.DeleteUserAsync(user.Id, true);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            _mockUnitOfWork.Verify(x => x.IUserRepository.Delete(It.IsAny<object>()), Times.Once);
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        [Fact]
        public async Task DeleteUserAsync_Successful_ReturnOk_WhenSaveToDbIsFalse()
        {
            // Arrange
            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.DeleteUserAsync(user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            _mockPendingChangesManager.Verify(x => x.DeleteToSessionJsonAsync(It.IsAny<JsonEntities>(), It.IsAny<ulong>(), It.IsAny<string>()), Times.Once);
            Log.Information("Verified if the DeleteUserAsync method triggered only once");
        }


        [Fact]
        public async Task DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
        {
            // Arrange
            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>()))
                    .ReturnsAsync(user);
            _mockUnitOfWork.Setup(x => x.IUserRepository.Delete(It.IsAny<object>()))
                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.DeleteUserAsync(user.Id, true));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // RetireUserAsync
        [Fact]
        public async Task RetireUserAsync_ShouldReturnNotFound_WhenInvalidUserId_IsPassed()
        {
            // Arrange
            ulong userId = _fixture.Create<ulong>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                .ReturnsAsync((Data.SQLCipher.User?)null);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.RetireUserAsync(userId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
            Log.Information("Verified if the result object's message is UserDoesNotExists");
            _mockUnitOfWork.Verify(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.IUserRepository.RetireUserAsync(It.IsAny<Data.SQLCipher.User>()), Times.Never);
        }


        [Fact]
        public async Task RetireUserAsync_ShouldReturConflict_WhenRetiredUserId_IsPassed()
        {
            // Arrange
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                            .With(x => x.Retired, true)
                            .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.RetireUserAsync(user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified if the result object's status code is Conflict:409");
            Assert.Equal(ResponseConstants.UserAlreadyRetired, result.Message);
            Log.Information("Verified if the result object's message is UserAlreadyRetired");
            _mockUnitOfWork.Verify(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.IUserRepository.RetireUserAsync(It.IsAny<Data.SQLCipher.User>()), Times.Never);
        }


        [Fact]
        public async Task RetireUserAsync_ShouldReturnOk_WhenValidUserId_IsPassed_WithSaveToDb_False()
        {
            // Arrange
            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.RetireUserAsync(user.Id);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is OK:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            _mockUnitOfWork.Verify(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            _mockPendingChangesManager.Verify(x => x.RetireToSessionJsonAsync(It.IsAny<JsonEntities>(), It.IsAny<ulong>(), It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task RetireUserAsync_ShouldReturnOk_WhenValidUserId_IsPassed_WithSaveToDb_True()
        {
            // Arrange
            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _userBL.RetireUserAsync(user.Id, true);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is OK:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
            _mockUnitOfWork.Verify(x => x.IUserRepository.GetById(It.IsAny<object>(), It.IsAny<Expression<Func<Data.SQLCipher.User, object>>[]>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.IUserRepository.RetireUserAsync(It.IsAny<Data.SQLCipher.User>()), Times.Once);
        }


        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // UnlockUserAsync
        [Fact]
        public async Task UnlockUserAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            ulong userId = 123UL;
            bool changePasswordOnLogin = true;
            _mockUnitOfWork.Setup(uow => uow.IUserRepository.GetById(userId)).ReturnsAsync((Data.SQLCipher.User)null);

            // Act
            ApiResponse response = await _userBL.UnlockUserAsync(userId, changePasswordOnLogin, saveToDb: true);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ResponseConstants.UserDoesNotExists, response.Message);
            _mockUnitOfWork.Verify(uow => uow.IUserRepository.GetById(userId), Times.Once);
        }

        [Fact]
        public async Task UnlockUserAsync_ShouldUnlockUserAndSaveToDb_WhenSaveToDbIsTrue()
        {
            // Arrange
            ulong userId = 123UL;
            bool changePasswordOnLogin = true;
            Data.SQLCipher.User mockUser = new Data.SQLCipher.User { Id = userId, UserName = "testuser" };

            _mockUnitOfWork.Setup(uow => uow.IUserRepository.GetById(userId)).ReturnsAsync(mockUser);
            _mockUnitOfWork.Setup(uow => uow.IUserRepository.UnlockUserAsync(mockUser, changePasswordOnLogin)).Returns(Task.CompletedTask);

            // Act
            ApiResponse response = await _userBL.UnlockUserAsync(userId, changePasswordOnLogin, saveToDb: true);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ResponseConstants.Success, response.Message);
            _mockUnitOfWork.Verify(uow => uow.IUserRepository.GetById(userId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.IUserRepository.UnlockUserAsync(mockUser, changePasswordOnLogin), Times.Once);
        }

        [Fact]
        public async Task UnlockUserAsync_ShouldUnlockUserToJson_WhenSaveToDbIsFalse()
        {
            // Arrange
            ulong userId = 123UL;
            bool changePasswordOnLogin = true;
            Data.SQLCipher.User mockUser = new Data.SQLCipher.User { Id = userId, UserName = "testuser" };

            _mockUnitOfWork.Setup(uow => uow.IUserRepository.GetById(userId)).ReturnsAsync(mockUser);
            _mockPendingChangesManager.Setup(manager => manager.UnlockToSessionJsonAsync(JsonEntities.User, mockUser.Id, mockUser.UserName, changePasswordOnLogin)).Returns(Task.CompletedTask);

            // Act
            ApiResponse response = await _userBL.UnlockUserAsync(userId, changePasswordOnLogin, saveToDb: false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ResponseConstants.Success, response.Message);
            _mockUnitOfWork.Verify(uow => uow.IUserRepository.GetById(userId), Times.Once);
            _mockPendingChangesManager.Verify(manager => manager.UnlockToSessionJsonAsync(JsonEntities.User, mockUser.Id, mockUser.UserName, changePasswordOnLogin), Times.Once);
        }

        // LoginAsync
        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserDoesNotExists()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync((Data.SQLCipher.User?)null);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
            Log.Information("Verified if the result object's message is UserDoesNotExists");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserIsLocked()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, true)
                .With(x => x.Retired, false)
                .With(x => x.PasswordExpiryEnable, false)
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.AccountLocked, result.Message);
            Log.Information("Verified if the result object's message is AccountLocked");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserIsRetired()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, true)
                .With(x => x.PasswordExpiryEnable, false)
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.AccountRetired, result.Message);
            Log.Information("Verified if the result object's message is AccountRetired");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserInactive()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsActive, false)
                .With(x => x.PasswordExpiryEnable, false)
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.AccountInActive, result.Message);
            Log.Information("Verified if the result object's message is AccountInActive");
        }



        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserPassword_IsExpired()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.PasswordExpiryEnable, true)
                .With(x => x.PasswordExpiryDate, DateTime.UtcNow.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.PasswordExpired, result.Message);
            Log.Information("Verified if the result object's message is PasswordExpired");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_OkResponse_ForLegacyUser_WhenFirstLogin_IsTrue()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsLegacy, true)
                .With(x => x.FirstLogin, true)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(loginRequest.Password);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                    .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_ForLegacyUser_WhenFirstLogin_IsTrue()
        {
            // Arrange
            string password = _fixture.Create<string>();
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsLegacy, true)
                .With(x => x.FirstLogin, true)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(password);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                    .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
            Log.Information("Verified if the result object's message is InvalidPassword");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_OkResponse_ForNonLegacyUser_WhenFirstLogin_IsTrue()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsLegacy, false)
                .With(x => x.FirstLogin, true)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.VerifyHashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                    .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_ForNonLegacyUser_WhenFirstLogin_IsTrue()
        {
            // Arrange
            string password = _fixture.Create<string>();
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsLegacy, false)
                .With(x => x.FirstLogin, true)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.VerifyHashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                    .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
            Log.Information("Verified if the result object's message is InvalidPassword");
        }



        [Fact]
        public async Task LoginAsync_ShouldReturn_OkResponse_ForLegacyUser_WhenFirstLogin_IsFalse()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsLegacy, true)
                .With(x => x.FirstLogin, false)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(loginRequest.Password);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                    .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        private static RSA GetRsa()
        {
            RSA rsa = RSA.Create(2048); // Specify the key size (2048 bits is common)
            string privateKeyPem = ExportPrivateKeyToPem(rsa);
            return rsa;
        }

        private static string ExportPrivateKeyToPem(RSA rsa)
        {
            byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
            string privateKeyPem = Convert.ToBase64String(privateKeyBytes);
            return $"-----BEGIN PRIVATE KEY-----\n{privateKeyPem}\n-----END PRIVATE KEY-----";
        }


        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_ForLegacyUsers_IfPasswordIsIncorrect_WhenFirstLogin_IsFalse()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            string dbPassword = _fixture.Create<string>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsActive, true)
                .With(x => x.IsLegacy, true)
                .With(x => x.FirstLogin, false)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
                .Returns(dbPassword);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
            Log.Information("Verified if the result object's message is InvalidPassword");
        }



        [Fact]
        public async Task LoginAsync_ShouldReturn_OkResponse_ForNonLegacyUser_WhenFirstLogin_IsFalse()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsActive, true)
                .With(x => x.IsLegacy, false)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.VerifyHashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                    .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                      .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
        }



        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_ForNonLegacyUsers_IfPasswordIsIncorrect_WhenFirstLogin_IsFalse()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsActive, true)
                .With(x => x.IsLegacy, false)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockEncryptionDecryption.Setup(x => x.VerifyHashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
            Log.Information("Verified if the result object's message is InvalidPassword");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_PasswordIsNotSetForUser()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
                .With(x => x.Locked, false)
                .With(x => x.Retired, false)
                .With(x => x.IsActive, true)
                .With(x => x.IsLegacy, false)
                .With(x => x.Password, string.Empty)
                .With(x => x.PasswordExpiryEnable, false)
                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
                .Create();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
            Log.Information("Verified if the result object's message is InvalidPassword");
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
        {
            // Arrange
            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.LoginAsync(loginRequest));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }



        // ValidateSessionAsync
        [Fact]
        public async Task ValidateSessionAsync_ShouldReturn_UnauthorizedResponse_SessionIdIsInvalid()
        {
            // Arrange
            string sessionId = _fixture.Create<string>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserBySessionId(sessionId))
                .ReturnsAsync(0ul);
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.ValidateSessionAsync(sessionId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result.Data);
            Log.Information("Verified if the response data object is null");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Log.Information("Verified if the result object's status code is Unauthorized:401");
            Assert.Equal(ResponseConstants.InvalidSessionId, result.Message);
            Log.Information("Verified if the result object's message is InvalidSessionId");
        }


        [Fact]
        public async Task ValidateSessionAsync_ShouldReturn_OkResponse_ValidSessionId()
        {
            // Arrange
            string sessionId = _fixture.Create<string>();
            ulong userId = _fixture.Create<ulong>();
            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserBySessionId(sessionId))
                .ReturnsAsync(userId);
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
                .ReturnsAsync(userDetails);
            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(Task.CompletedTask);
            _mockRsaBl.Setup(x => x.GetPrivateKey())
                        .Returns(GetRsa());
            Log.Information("Completed Moqing dependencies");

            // Act
            ServiceResponse<LoginServiceResponse> result = await _userBL.ValidateSessionAsync(sessionId);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result.Data);
            Log.Information("Verified if the response data object is not null");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        [Fact]
        public async Task ValidateSessionAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
        {
            // Arrange
            string sessionId = _fixture.Create<string>();
            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserBySessionId(sessionId))
                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
            Log.Information("Completed Moqing dependencies");

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _userBL.ValidateSessionAsync(sessionId));
            Log.Information("Verified if the method throwing SqliteException for any db related issues");
        }

        // Test 1: Valid Session ID with Active Session
        [Fact]
        public async Task Logout_ValidSessionId_ReturnsNoContent()
        {
            // Arrange
            string validSessionId = "session-12345";
            _mockUnitOfWork.Setup(x => x.IUserRepository.ClearUserSessionAsync(validSessionId))
                .ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            bool result = await _userBL.ClearUserSessionAsync(validSessionId);

            // Assert
            Assert.True(result); // Assert that the result is true and Nocontent
            Log.Information("Session with ID {SessionId} has been successfully logged out.", validSessionId);
            Log.Information("Verified if the valid session is properly logged out");
        }

        // Negative Test Scenario
        // Test 2: Invalid Session ID (Session not found)
        [Fact]
        public async Task Logout_InvalidSessionId_ReturnsFalse()
        {
            // Arrange
            string invalidSessionId = "invalid-session";
            _mockUnitOfWork.Setup(x => x.IUserRepository.ClearUserSessionAsync(invalidSessionId)).ReturnsAsync(false);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            bool result = await _userBL.ClearUserSessionAsync(invalidSessionId);

            // Assert
            Assert.False(result);
            Log.Information("Verified if the method returns true for the invalid session ID: {SessionId}", invalidSessionId);
        }

        // Positive Test Scenario
        // Test 3: Valid Session ID, but No Active Session
        [Fact]
        public async Task Logout_ValidSessionId_NoSession_ReturnsFalse()
        {
            // Arrange
            string validSessionId = "session-12345";
            _mockUnitOfWork.Setup(x => x.IUserRepository.ClearUserSessionAsync(validSessionId))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            bool result = await _userBL.ClearUserSessionAsync(validSessionId);

            // Assert
            Assert.False(result);
            Log.Information("Verified if the method returns NotFound for a valid session ID with no associated active session: {SessionId}", validSessionId);
        }

        // Negative Test Scenario
        // Test 4: Empty Session ID Provided
        [Fact]
        public async Task Logout_EmptySessionId_ReturnsFalse()
        {
            // Arrange
            string emptySessionId = string.Empty;
            _mockUnitOfWork.Setup(x => x.IUserRepository.ClearUserSessionAsync(emptySessionId))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            bool result = await _userBL.ClearUserSessionAsync(emptySessionId);

            // Assert
            Assert.False(result);
            Log.Information("Verified if the method returns false for an empty session ID.");
        }

        // Negative Test Scenario
        // Test 5: Null Session ID Provided
        [Fact]
        public async Task Logout_NullSessionId_ReturnsFalse()
        {
            // Arrange
            string? nullSessionId = null;
            _mockUnitOfWork.Setup(x => x.IUserRepository.ClearUserSessionAsync(It.Is<string>(id => id == null)))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(x => x.IEventLogRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<string>()))
                 .Returns(Task.CompletedTask);

            // Act
            bool result = await _userBL.ClearUserSessionAsync(nullSessionId!);

            // Assert
            Assert.False(result);
            Log.Information("Verified if the method returns NotFound for a null session ID");
        }


        // UpdateUserLanguageAsync
        [Fact]
        public async Task UpdateUserLanguageAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string language = "en";
            ulong userId = 1;

            Log.Information("Starting test: UpdateUserLanguageAsync_ReturnsNotFound_WhenUserDoesNotExist");

            _mockCurrentUserService.Setup(c => c.UserDetails).Returns(new UserDetails { Id = userId });
            _mockUnitOfWork.Setup(u => u.IUserRepository.GetById(userId)).ReturnsAsync((Data.SQLCipher.User?)null);

            // Log the request
            Log.Information("UpdateUserLanguageAsync method request: UserId={UserId}, Language={Language}", userId, language);

            // Act
            ApiResponse result = await _userBL.UpdateUserLanguageAsync(language);

            // Log the result
            Log.Information("UpdateUserLanguageAsync method response: StatusCode={StatusCode}, Message={Message}", result.StatusCode, result.Message);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);

            // Log assertion passed
            Log.Information("Assertion passed: StatusCode={StatusCode}, Message={Message} match expected values.", result.StatusCode, result.Message);

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task UpdateUserLanguageAsync_ReturnsSuccess_WhenUserExistsAndLanguageIsUpdated()
        {
            // Arrange
            string language = "en";
            ulong userId = 1;
            Data.SQLCipher.User user = new Data.SQLCipher.User { Id = userId };

            Log.Information("Starting test: UpdateUserLanguageAsync_ReturnsSuccess_WhenUserExistsAndLanguageIsUpdated");

            _mockCurrentUserService.Setup(c => c.UserDetails).Returns(new UserDetails { Id = userId });
            _mockUnitOfWork.Setup(u => u.IUserRepository.GetById(userId)).ReturnsAsync(user);
            _mockUnitOfWork.Setup(u => u.IUserRepository.UpdateUserLanguageAsync(userId, language)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Log the request
            Log.Information("UpdateUserLanguageAsync method request: UserId={UserId}, Language={Language}", userId, language);

            // Act
            ApiResponse result = await _userBL.UpdateUserLanguageAsync(language);

            // Log the result
            Log.Information("UpdateUserLanguageAsync method response: StatusCode={StatusCode}, Message={Message}", result.StatusCode, result.Message);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(ResponseConstants.Success, result.Message);

            // Log assertion passed
            Log.Information("Assertion passed: StatusCode={StatusCode}, Message={Message} match expected values.", result.StatusCode, result.Message);

            // Verify the method calls and log them
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            Log.Information("Verified that BeginTransactionAsync was called once.");

            _mockUnitOfWork.Verify(u => u.IUserRepository.UpdateUserLanguageAsync(userId, language), Times.Once);
            Log.Information("Verified that UpdateUserLanguageAsync was called once.");

            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
            Log.Information("Verified that CommitTransactionAsync was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }
    }
}
