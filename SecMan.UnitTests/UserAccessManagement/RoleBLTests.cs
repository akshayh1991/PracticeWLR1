using Moq;
using SecMan.BL;
using SecMan.BL.Common;
using SecMan.Data.DAL;
using SecMan.Data.Repository;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class RoleBLTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPendingChangesManager> _mockPendingChangesManager;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly RoleBL _roleBL;

        public RoleBLTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPendingChangesManager = new Mock<IPendingChangesManager>();
            _mockRoleRepository = new Mock<IRoleRepository>();

            // Setup UnitOfWork to return our mocked RoleRepository
            _mockUnitOfWork.Setup(uow => uow.IRoleRepository).Returns(_mockRoleRepository.Object);

            _roleBL = new RoleBL(_mockUnitOfWork.Object, _mockPendingChangesManager.Object);

            // Initialize logger

        }

        [Fact]
        public async Task AddRoleAsync_ShouldReturnSuccess_WhenRoleIsAddedToDatabase()
        {
            // Arrange
            CreateRole newRole = new CreateRole
            {
                Name = "Admin",
                Description = "Admin Role",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2 }
            };

            Data.SQLCipher.Role addedRole = new Data.SQLCipher.Role
            {
                Id = 1,
                Name = newRole.Name,
                Description = newRole.Description,
                IsLoggedOutType = newRole.IsLoggedOutType
            };

            Log.Information("Setting up mock responses for AddRoleAsync.");
            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.IsRoleNameExistsForCreationAsync(newRole.Name))
                .ReturnsAsync(false);

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.ValidateLinkUsersAsync(newRole.LinkUsers))
                .ReturnsAsync(true);

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.AddRoleAsync(newRole))
                .ReturnsAsync(addedRole);

            // Act
            Log.Information("Calling AddRoleAsync with saveToDb set to true.");
            ServiceResponse<GetRoleDto> result = await _roleBL.AddRoleAsync(newRole, saveToDb: true);

            // Assert
            Log.Information("Asserting the response from AddRoleAsync.");
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(ResponseConstants.Success, result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(addedRole.Id, result.Data.Id);
            Assert.Equal(addedRole.Name, result.Data.Name);
            Assert.Equal(addedRole.Description, result.Data.Description);
            Log.Information("AddRoleAsync test passed with success response.");
        }

        [Fact]
        public async Task AddRoleAsync_ShouldReturnBadRequest_WhenRoleNameAlreadyExists()
        {
            // Arrange
            CreateRole newRole = new CreateRole { Name = "Admin" };

            Log.Information("Setting up mock responses for existing role name.");
            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.IsRoleNameExistsForCreationAsync(newRole.Name))
                .ReturnsAsync(true);

            // Act
            Log.Information("Calling AddRoleAsync with an existing role name.");
            ServiceResponse<GetRoleDto> result = await _roleBL.AddRoleAsync(newRole, saveToDb: true);

            // Assert
            Log.Information("Asserting the response for existing role name.");
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(RoleResponseConstants.RoleAlreadyExists, result.Message);
            Assert.Null(result.Data);
            Log.Information("AddRoleAsync test passed for existing role name scenario.");
        }

        [Fact]
        public async Task AddRoleAsync_ShouldReturnBadRequest_WhenLinkedUsersAreInvalid()
        {
            // Arrange
            CreateRole newRole = new CreateRole
            {
                Name = "Manager",
                LinkUsers = new List<ulong> { 999 } // Invalid user IDs
            };

            Log.Information("Setting up mock responses for invalid linked users.");
            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.IsRoleNameExistsForCreationAsync(newRole.Name))
                .ReturnsAsync(false);

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.ValidateLinkUsersAsync(newRole.LinkUsers))
                .ReturnsAsync(false);

            // Act
            Log.Information("Calling AddRoleAsync with invalid linked users.");
            ServiceResponse<GetRoleDto> result = await _roleBL.AddRoleAsync(newRole, saveToDb: true);

            // Assert
            Log.Information("Asserting the response for invalid linked users.");
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(RoleResponseConstants.InvalidUserIds, result.Message);
            Assert.Null(result.Data);
            Log.Information("AddRoleAsync test passed for invalid linked users scenario.");
        }

        [Fact]
        public async Task AddRoleAsync_ShouldAddRoleToSessionJson_WhenSaveToDbIsFalse()
        {
            // Arrange
            CreateRole newRole = new CreateRole { Name = "Guest" };

            ServiceResponse<object> pendingChangesResponse = new ServiceResponse<object>
            {
                Message = "Added to session JSON",
                StatusCode = HttpStatusCode.Accepted
            };

            Log.Information("Setting up mock responses for adding role to session JSON.");
            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.IsRoleNameExistsForCreationAsync(newRole.Name))
                .ReturnsAsync(false);

            _mockPendingChangesManager
                .Setup(manager => manager.AddToSessionJsonAsync(newRole, SecMan.BL.Common.JsonEntities.Role))
                .ReturnsAsync(pendingChangesResponse);

            // Act
            Log.Information("Calling AddRoleAsync with saveToDb set to false.");
            ServiceResponse<GetRoleDto> result = await _roleBL.AddRoleAsync(newRole, saveToDb: false);

            // Assert
            Log.Information("Asserting the response for session JSON addition.");
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
            Assert.Equal("Added to session JSON", result.Message);
            Assert.Null(result.Data);
            Log.Information("AddRoleAsync test passed for session JSON addition scenario.");
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenUserIdsAreInvalid()
        {
            // Arrange
            ulong roleId = 1UL;
            UpdateRole updateRoleDto = new UpdateRole
            {
                Name = "NewRoleName",
                LinkUsers = new List<ulong> { 999UL } // Invalid user ID
            };

            // Log the arrangement step
            Log.Information("Arranging test: Validating user IDs for role update.");

            _mockRoleRepository.Setup(r => r.ValidateLinkUsersAsync(updateRoleDto.LinkUsers))
                .ReturnsAsync(false);

            // Act
            Log.Information("Acting: Calling UpdateRoleAsync for role ID {RoleId} with user IDs {UserIds}.", roleId, string.Join(",", updateRoleDto.LinkUsers));
            ServiceResponse<GetRoleDto?> response = await _roleBL.UpdateRoleAsync(roleId, updateRoleDto);

            // Assert using xUnit's Assert methods
            Log.Information("Asserting: Verifying response status code and message.");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Check StatusCode
            Assert.Contains("Some provided user IDs do not exist", response.Message); // Update to match the actual error message
            Assert.Null(response.Data); // Ensure Data is null

            Log.Information("Test completed: Invalid user IDs, returning BadRequest.");
        }
        [Fact]
        public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenRoleNameAlreadyExists()
        {
            // Arrange
            ulong roleId = 1;
            UpdateRole updateRoleDto = new UpdateRole { Name = "ExistingRole" };

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.IsRoleNameExistsAsync(roleId, updateRoleDto.Name))
                .ReturnsAsync(true);

            Log.Information("Starting test: UpdateRoleAsync_ShouldReturnBadRequest_WhenRoleNameAlreadyExists");
            Log.Information("Checking if role name '{RoleName}' already exists for RoleId: {RoleId}", updateRoleDto.Name, roleId);

            // Act
            ServiceResponse<GetRoleDto?> result = await _roleBL.UpdateRoleAsync(roleId, updateRoleDto, saveToDb: true);

            // Assert
            Log.Information("Test completed with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(RoleResponseConstants.RoleAlreadyExists, result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenLinkedUsersAreInvalid()
        {
            // Arrange
            ulong roleId = 1;
            UpdateRole updateRoleDto = new UpdateRole
            {
                Name = "NewRole",
                LinkUsers = new List<ulong> { 999 } // Invalid user IDs
            };

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.IsRoleNameExistsAsync(roleId, updateRoleDto.Name))
                .ReturnsAsync(false);

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.ValidateLinkUsersAsync(updateRoleDto.LinkUsers))
                .ReturnsAsync(false);

            Log.Information("Starting test: UpdateRoleAsync_ShouldReturnBadRequest_WhenLinkedUsersAreInvalid");
            Log.Information("Validating linked user IDs: {LinkUsers} for RoleId: {RoleId}", updateRoleDto.LinkUsers, roleId);

            // Act
            ServiceResponse<GetRoleDto?> result = await _roleBL.UpdateRoleAsync(roleId, updateRoleDto, saveToDb: true);

            // Assert
            Log.Information("Test completed with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(RoleResponseConstants.InvalidUserIds, result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldDeleteRoleFromDatabase_WhenSaveToDbIsTrue()
        {
            // Arrange
            ulong roleId = 1;
            Data.SQLCipher.Role role = new Data.SQLCipher.Role { Id = roleId, Name = "TestRole" };

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.GetById(roleId))
                .ReturnsAsync(role);

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.Delete(roleId))
                .ReturnsAsync(true);

            Log.Information("Starting test: DeleteRoleAsync_ShouldDeleteRoleFromDatabase_WhenSaveToDbIsTrue");
            Log.Information("Fetching role with RoleId: {RoleId}", roleId);

            // Act
            ApiResponse result = await _roleBL.DeleteRoleAsync(roleId, saveToDb: true);

            // Assert
            Log.Information("Test completed with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(ResponseConstants.Success, result.Message);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldDeleteRoleFromSessionJson_WhenSaveToDbIsFalse()
        {
            // Arrange
            ulong roleId = 1;
            Data.SQLCipher.Role role = new Data.SQLCipher.Role { Id = roleId, Name = "TestRole" };

            _mockUnitOfWork
                .Setup(uow => uow.IRoleRepository.GetById(roleId))
                .ReturnsAsync(role);

            _mockPendingChangesManager
                .Setup(manager => manager.DeleteToSessionJsonAsync(JsonEntities.Role, role.Id, role.Name))
                .Returns(Task.CompletedTask);

            Log.Information("Starting test: DeleteRoleAsync_ShouldDeleteRoleFromSessionJson_WhenSaveToDbIsFalse");
            Log.Information("Fetching role with RoleId: {RoleId}", roleId);

            // Act
            ApiResponse result = await _roleBL.DeleteRoleAsync(roleId, saveToDb: false);

            // Assert
            Log.Information("Test completed with StatusCode: {StatusCode}, Message: {Message}", result.StatusCode, result.Message);
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(ResponseConstants.Success, result.Message);
        }

    }
}
