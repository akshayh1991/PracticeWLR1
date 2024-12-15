using Microsoft.EntityFrameworkCore;
using SecMan.Data.DAL;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;

namespace SecMan.UnitTests.DAL
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class RoleRepositoryTests
    {
        private readonly DbContextOptions<Db> _dbContextOptions;
        private readonly Db _dbContext;
        private readonly RoleRepository _roleRepository;

        public RoleRepositoryTests()
        {
            // Configure the in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<Db>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            // Provide a placeholder databaseFile argument
            string testDatabaseFile = "TestDbFile.db";

            // Initialize the Db context with the required parameters
            _dbContext = new Db(_dbContextOptions, testDatabaseFile);

            // Initialize the repository
            _roleRepository = new RoleRepository(_dbContext);
        }



        [Fact]
        public async Task IsRoleNameExistsForCreationAsync_ReturnsTrue_WhenRoleNameExists()
        {
            Log.Information("Starting test: IsRoleNameExistsForCreationAsync_ReturnsTrue_WhenRoleNameExists");

            // Arrange: Add a role to the in-memory database
            Log.Information("Adding a role with the name 'Admin' to the in-memory database.");
            Role existingRole = new SecMan.Data.SQLCipher.Role { Name = "Admin" };
            _dbContext.Roles.Add(existingRole);
            await _dbContext.SaveChangesAsync();

            // Act: Check if the role name exists
            Log.Information("Calling IsRoleNameExistsForCreationAsync with role name 'Admin'.");
            bool result = await _roleRepository.IsRoleNameExistsForCreationAsync("Admin");

            // Assert: Verify that the role name exists
            Log.Information("Asserting that the role name exists.");
            Assert.True(result);

            Log.Information("Test IsRoleNameExistsForCreationAsync_ReturnsTrue_WhenRoleNameExists completed successfully.");
        }

        [Fact]
        public async Task IsRoleNameExistsForCreationAsync_ReturnsFalse_WhenRoleNameDoesNotExist()
        {
            Log.Information("Starting test: IsRoleNameExistsForCreationAsync_ReturnsFalse_WhenRoleNameDoesNotExist");

            // Arrange: Use a non-existent role name
            string nonExistentRoleName = "User";
            Log.Information("Using a non-existent role name: 'User'.");

            // Act: Check if the role name exists
            Log.Information($"Calling IsRoleNameExistsForCreationAsync with role name '{nonExistentRoleName}'.");
            bool result = await _roleRepository.IsRoleNameExistsForCreationAsync(nonExistentRoleName);

            // Assert: Verify that the role name does not exist
            Log.Information("Asserting that the role name does not exist.");
            Assert.False(result);

            Log.Information("Test IsRoleNameExistsForCreationAsync_ReturnsFalse_WhenRoleNameDoesNotExist completed successfully.");
        }

        [Fact]
        public async Task ValidateLinkUsersAsync_ReturnsTrue_WhenAllUsersExist()
        {
            Log.Information("Starting test: ValidateLinkUsersAsync_ReturnsTrue_WhenAllUsersExist");

            // Arrange: Add users to the in-memory database
            List<ulong> userIds = new List<ulong> { 1, 2, 3 };
            Log.Information("Adding users with IDs 1, 2, 3 to the in-memory database.");
            _dbContext.Users.AddRange(new SecMan.Data.SQLCipher.User { Id = 1 }, new SecMan.Data.SQLCipher.User { Id = 2 }, new SecMan.Data.SQLCipher.User { Id = 3 });
            await _dbContext.SaveChangesAsync();

            // Act: Validate if all users exist
            Log.Information("Calling ValidateLinkUsersAsync with user IDs 1, 2, 3.");
            bool result = await _roleRepository.ValidateLinkUsersAsync(userIds);

            // Assert: Verify that all users exist
            Log.Information("Asserting that all users exist.");
            Assert.True(result);

            Log.Information("Test ValidateLinkUsersAsync_ReturnsTrue_WhenAllUsersExist completed successfully.");
        }


        [Fact]
        public async Task ValidateLinkUsersAsync_ReturnsFalse_WhenNoUsersExist()
        {
            Log.Information("Starting test: ValidateLinkUsersAsync_ReturnsFalse_WhenNoUsersExist");

            // Arrange: Use a list of user IDs that do not exist
            List<ulong> userIds = new List<ulong> { 999, 1000, 1001 };
            Log.Information("Using non-existent user IDs 999, 1000, 1001.");

            // Act: Validate if the users exist
            Log.Information("Calling ValidateLinkUsersAsync with non-existent user IDs 999, 1000, 1001.");
            bool result = await _roleRepository.ValidateLinkUsersAsync(userIds);

            // Assert: Verify that no users exist
            Log.Information("Asserting that no users exist.");
            Assert.False(result);

            Log.Information("Test ValidateLinkUsersAsync_ReturnsFalse_WhenNoUsersExist completed successfully.");
        }


        [Fact]
        public async Task AddRoleAsync_ShouldThrowException_WhenNoUserIdsProvided()
        {
            Log.Information("Starting test: AddRoleAsync_ShouldThrowException_WhenNoUserIdsProvided");

            // Arrange: Prepare data with no linked users
            Log.Information("Preparing role creation data with no user IDs.");
            CreateRole addRoleDto = new CreateRole
            {
                Name = "Viewer",
                Description = "Viewer role",
                IsLoggedOutType = false,
                LinkUsers = new List<ulong>() // Empty list
            };

            // Act & Assert: Expecting no users to be linked
            Log.Information("Calling AddRoleAsync method and expecting an empty user list.");
            Role result = await _roleRepository.AddRoleAsync(addRoleDto);

            Log.Information("Asserting that no users are linked to the added role.");
            Assert.NotNull(result);
            Assert.Empty(result.Users);
            Assert.Equal("Viewer", result.Name);

            Log.Information("Test AddRoleAsync_ShouldThrowException_WhenNoUserIdsProvided completed successfully.");
        }

        [Fact]
        public async Task UpdateRoleFromJsonAsync_ReturnsNull_WhenRoleDoesNotExist()
        {
            Log.Information("Starting test: UpdateRoleFromJsonAsync_ReturnsNull_WhenRoleDoesNotExist");

            // Arrange
            Log.Information("Ensuring no roles exist in the in-memory database for testing.");
            UpdateRole updateDto = new UpdateRole
            {
                Name = "Updated Admin",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 101, 102 }
            };

            // Act
            Log.Information("Calling UpdateRoleFromJsonAsync with non-existent role ID 99.");
            GetRoleDto result = await _roleRepository.UpdateRoleFromJsonAsync(99, updateDto);

            // Assert
            Log.Information("Asserting that the result is null when the role does not exist.");
            Assert.Null(result);

            Log.Information("Test UpdateRoleFromJsonAsync_ReturnsNull_WhenRoleDoesNotExist completed successfully.");
        }


        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenRoleDoesNotExist()
        {
            Log.Information("Starting test: DeleteAsync_ReturnsFalse_WhenRoleDoesNotExist");

            // Act
            Log.Information("Calling DeleteAsync with non-existent role ID 99.");
            bool result = await _roleRepository.DeleteAsync(99);

            // Assert
            Log.Information("Asserting that the result is false when the role does not exist.");
            Assert.False(result);

            Log.Information("Test DeleteAsync_ReturnsFalse_WhenRoleDoesNotExist completed successfully.");
        }
    }
}
