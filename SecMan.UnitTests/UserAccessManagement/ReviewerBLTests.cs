using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using SecMan.BL;
using SecMan.Data.Exceptions;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using System.IO.Abstractions;
using System.Net;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class ReviewerBLTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly IReviewerBl _reviewerBl;
        private readonly Mock<IUserBL> _mockUserBL;
        private readonly Mock<IRoleBL> _mockRoleBL;
        private readonly Mock<ISystemFeatureBL> _mockSystemFeatureBL;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IDeviceBL> _mockDeviceBL;
        private readonly IFixture _fixture;


        public ReviewerBLTests()
        {
            _fixture = new Fixture();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUserBL = new Mock<IUserBL>();
            _mockRoleBL = new Mock<IRoleBL>();
            _mockSystemFeatureBL = new Mock<ISystemFeatureBL>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockFileSystem = new Mock<IFileSystem>();
            _reviewerBl = new ReviewerBl(
                    _mockUserBL.Object,
                    _mockUnitOfWork.Object,
                    _mockConfiguration.Object,
                    _mockSystemFeatureBL.Object,
                    _mockRoleBL.Object,
                    _mockHttpContextAccessor.Object,
                    _mockFileSystem.Object,
                    _mockDeviceBL.Object
                );
        }



        // ReadJsonData
        [Fact]
        public async Task ReadJsonData_ReturnsSuccess_WhenFileExists()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            string jsonContent = "{\"key\": \"value\"}";

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));

            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(jsonContent);

            // Act
            ServiceResponse<JObject> result = await _reviewerBl.ReadJsonData();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("value", result.Data["key"].ToString());
        }


        [Fact]
        public async Task ReadJsonData_ReturnsSuccess_WhenFileExists_WithEmptyObject()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            string jsonContent = "{}";

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));

            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(jsonContent);

            // Act
            ServiceResponse<JObject> result = await _reviewerBl.ReadJsonData();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("{}", result.Data.ToString());
        }


        [Fact]
        public async Task ReadJsonData_ReturnsSuccess_WhenFileNotExists_WithEmptyObject()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            string jsonContent = "";

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));

            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fs => fs.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(jsonContent);

            // Act
            ServiceResponse<JObject> result = await _reviewerBl.ReadJsonData();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(result.Data);
        }



        // SaveUnsavedJsonChanges
        [Fact]
        public async Task SaveUnsavedJsonChanges_ReturnsSuccess_WhenDirectoryNotExists()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject jsonContent = new JObject
            {
                ["key"] = "value"
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(false);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedJsonChanges(jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }


        [Fact]
        public async Task SaveUnsavedJsonChanges_ReturnsSuccess_WhenDirectoryExists_AndFileDoesNotExists()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject jsonContent = new JObject
            {
                ["key"] = "value"
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(false);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedJsonChanges(jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }


        [Fact]
        public async Task SaveUnsavedJsonChanges_ReturnsSuccess_WhenDirectoryExists_AndFileExists()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject jsonContent = new JObject
            {
                ["key"] = "value"
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedJsonChanges(jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }


        [Fact]
        public async Task SaveUnsavedJsonChanges_ReturnsSuccess_WhenJObjectIsNull()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedJsonChanges(jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }


        // SaveUnsavedChanges
        [Fact]
        public async Task SaveUnsavedChanges_MultipleChanges_ThrowsInvalidOperationException()
        {
            // Arrange
            UserUnsavedChanges userChanges = _fixture.Create<UserUnsavedChanges>();
            userChanges.Create = _fixture.CreateMany<object>(2).ToList();
            UnsavedChanges roleChanges = _fixture.Create<UnsavedChanges>();
            roleChanges.Create = _fixture.CreateMany<object>(1).ToList();
            JObject inputModel = JObject.FromObject(new
            {
                users = userChanges,
                roles = roleChanges
            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reviewerBl.SaveUnsavedChanges(inputModel));
        }


        [Fact]
        public async Task SaveUnsavedChanges_NoChanges_CommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));

            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
            JObject inputModel = JObject.FromObject(new
            {
                users = new UserUnsavedChanges(),
                roles = new UnsavedChanges(),
                settingAndPolicies = new SystemFeaturesUnSavedChanges()
            });

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedChanges(inputModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }


        [Fact]
        public async Task SaveUnsavedChanges_ValidUserChanges_CommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);

            UserUnsavedChanges userUnsavedChanges = _fixture.Create<UserUnsavedChanges>();
            userUnsavedChanges.Create = _fixture.CreateMany<object>(5).ToList();
            _mockUserBL.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.UnlockUserAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));

            UsersJsonData usersJsonData = new UsersJsonData { Users = userUnsavedChanges };
            JObject inputModel = JObject.FromObject(usersJsonData);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedChanges(inputModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }


        [Fact]
        public async Task SaveUnsavedChanges_InValidUserChanges_DoesNotCommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            UserUnsavedChanges userUnsavedChanges = _fixture.Create<UserUnsavedChanges>();
            userUnsavedChanges.Create = _fixture.CreateMany<object>(5).ToList();
            _mockUserBL.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest));
            _mockUserBL.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.UnlockUserAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.RetireUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
            _mockUserBL.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));

            UsersJsonData usersJsonData = new UsersJsonData { Users = userUnsavedChanges };
            JObject inputModel = JObject.FromObject(usersJsonData);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedChanges(inputModel);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }



        [Fact]
        public async Task SaveUnsavedChanges_ValidRoleChanges_CommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));

            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
            UnsavedChanges roleUnsavedChanges = _fixture.Create<UnsavedChanges>();
            roleUnsavedChanges.Create = _fixture.CreateMany<object>(5).ToList();
            _mockRoleBL.Setup(x => x.AddRoleAsync(It.IsAny<CreateRole>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<GetRoleDto>(ResponseConstants.Success, HttpStatusCode.Created));
            _mockRoleBL.Setup(x => x.UpdateRoleAsync(It.IsAny<ulong>(), It.IsAny<UpdateRole>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<GetRoleDto>(ResponseConstants.Success, HttpStatusCode.OK));
            _mockRoleBL.Setup(x => x.DeleteRoleAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));

            RolesJsonData rolesJsonData = new RolesJsonData { Roles = roleUnsavedChanges };
            JObject inputModel = JObject.FromObject(rolesJsonData);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedChanges(inputModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }


        [Fact]
        public async Task SaveUnsavedChanges_InValiRoleChanges_DoesNotCommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            UnsavedChanges roleUnsavedChanges = _fixture.Create<UnsavedChanges>();
            roleUnsavedChanges.Create = _fixture.CreateMany<object>(5).ToList();
            _mockRoleBL.Setup(x => x.AddRoleAsync(It.IsAny<CreateRole>(), It.IsAny<bool>()))
                .ReturnsAsync(new ServiceResponse<GetRoleDto>());
            _mockRoleBL.Setup(x => x.UpdateRoleAsync(It.IsAny<ulong>(), It.IsAny<UpdateRole>(), It.IsAny<bool>()))
                .ThrowsAsync(new UpdatingExistingNameException("A role with the same name already exists."));
            _mockRoleBL.Setup(x => x.DeleteRoleAsync(It.IsAny<ulong>(), It.IsAny<bool>()))
                .ReturnsAsync(new ApiResponse());

            RolesJsonData rolesJsonData = new RolesJsonData { Roles = roleUnsavedChanges };
            JObject inputModel = JObject.FromObject(rolesJsonData);

            // Act && Assert
            await Assert.ThrowsAsync<UpdatingExistingNameException>(() => _reviewerBl.SaveUnsavedChanges(inputModel));
        }



        [Fact]
        public async Task SaveUnsavedChanges_ValidSystemFeatureChanges_CommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));

            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
            ModelStateDictionary modelState = new ModelStateDictionary();

            SystemFeaturesUnSavedChanges systemFeatureChanges = _fixture.Create<SystemFeaturesUnSavedChanges>();
            systemFeatureChanges.Update = _fixture.CreateMany<SystemFeaturesUpdateData>(3).ToList();
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(), It.IsAny<List<UpdateSystemPolicyData>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(new ServiceResponse<List<UpdatedResponse>>(ResponseConstants.Success, HttpStatusCode.OK));

            SystemFeatureJsonData systemFeatureJsonData = new SystemFeatureJsonData { Settings = systemFeatureChanges };
            JObject inputModel = JObject.FromObject(systemFeatureJsonData);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedChanges(inputModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }



        [Fact]
        public async Task SaveUnsavedChanges_InValidSystemFeatureChanges_DoesNotCommitsTransaction()
        {
            // Arrange
            string sessionId = "testSessionId";
            string jsonFilePath = $"C:/path/to/{sessionId}.json";
            JObject? jsonContent = null;
            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");
            _mockHttpContextAccessor.Setup(h => h.HttpContext.User.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("SSOSessionId", sessionId));
            SystemFeaturesUnSavedChanges systemFeatureChanges = _fixture.Create<SystemFeaturesUnSavedChanges>();
            systemFeatureChanges.Update = _fixture.CreateMany<SystemFeaturesUpdateData>(3).ToList();
            _mockSystemFeatureBL.Setup(x => x.UpdateSystemPolicyByIdAsync(It.IsAny<ulong>(), It.IsAny<List<UpdateSystemPolicyData>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(new ServiceResponse<List<UpdatedResponse>>(ValidationConstants.InvalidBoolean, HttpStatusCode.BadRequest));

            SystemFeatureJsonData systemFeatureJsonData = new SystemFeatureJsonData { Settings = systemFeatureChanges };
            JObject inputModel = JObject.FromObject(systemFeatureJsonData);

            // Act
            ApiResponse result = await _reviewerBl.SaveUnsavedChanges(inputModel);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }
    }
}
