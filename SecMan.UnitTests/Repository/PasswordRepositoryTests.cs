using Microsoft.EntityFrameworkCore;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;

namespace SecMan.UnitTests.Repository
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class PasswordRepositoryTests : IDisposable
    {
        private readonly PasswordRepository _passwordRepository;
        private readonly Db _dbContext;

        public PasswordRepositoryTests()
        {
            // Setup in-memory database for testing
            DbContextOptions<Db> options = new DbContextOptionsBuilder<Db>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _dbContext = new Db(options, string.Empty);
            _dbContext.Database.EnsureCreated();

            // Initialize the PasswordRepository with the in-memory context
            _passwordRepository = new PasswordRepository(_dbContext);
        }

        public void Dispose()
        {
            // Clean up the database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetUserCredentials_ShouldReturnUserCredentials_WhenUserExists()
        {
            // Arrange
            string userName = "testuser";
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = 1,
                UserName = userName,
                Password = "hashedPassword"
            };

            // Clear the database
            Log.Information("Clearing existing users from the database.");
            _dbContext.Users.RemoveRange(_dbContext.Users);
            await _dbContext.SaveChangesAsync();

            // Add the user to the in-memory database
            Log.Information("Adding a new user with username: {UserName}", userName);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            Log.Information("Acting to retrieve credentials for user: {UserName}", userName);
            UserCredentialsDto result = await _passwordRepository.GetUserCredentials(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.userId);
            Assert.Equal("hashedPassword", result.Password);

            Log.Information("Retrieved user credentials successfully for username: {UserName}", userName);
        }

        [Fact]
        public async Task GetUserCredentials_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            string userName = "nonexistentuser";
            Log.Information("Testing GetUserCredentials with a nonexistent username: {UserName}", userName);

            // Act
            UserCredentialsDto result = await _passwordRepository.GetUserCredentials(userName);

            // Assert
            Assert.Null(result);
            Log.Information("GetUserCredentials returned null as expected for username: {UserName}", userName);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldUpdatePassword_WhenUserExists()
        {
            // Arrange
            ulong userId = 1;
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = userId,
                UserName = "testuser",
                Password = "oldPassword"
            };

            // Log the setup
            Log.Information("Setting up test for UpdatePasswordAsync with userId: {UserId}, existing password: {OldPassword}", userId, user.Password);

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            string newPassword = "NewPassword123!";
            Log.Information("Testing UpdatePasswordAsync with new password: {NewPassword}", newPassword);

            // Act
            string updatedPassword = await _passwordRepository.UpdatePasswordAsync(userId, newPassword);

            // Assert
            Data.SQLCipher.User? updatedUser = await _dbContext.Users.FindAsync(userId); // Retrieve the user again to check the updated password
            Assert.Equal(newPassword, updatedPassword);
            Assert.Equal(newPassword, updatedUser.Password); // Verify that the user's password has been updated

            Log.Information("Password updated successfully for userId: {UserId}. New Password: {NewPassword}", userId, updatedPassword);
        }
        [Fact]
        public async Task GetRecentPasswordsAsync_ShouldReturnPasswords_WhenUserHasHistory()
        {
            // Arrange
            ulong userId = 1;
            List<PasswordHistory> passwordHistories = new List<PasswordHistory>
        {
            new PasswordHistory { UserId = userId, Password = "Password1", CreatedDate = DateTime.UtcNow.AddDays(-1) },
            new PasswordHistory { UserId = userId, Password = "Password2", CreatedDate = DateTime.UtcNow.AddDays(-2) },
            new PasswordHistory { UserId = userId, Password = "Password3", CreatedDate = DateTime.UtcNow.AddDays(-3) }
        };

            SysFeatProp sysFeatProps = new SysFeatProp { Name = "History", Val = "2" };

            _dbContext.PasswordHistories.AddRange(passwordHistories);
            _dbContext.SysFeatProps.Add(sysFeatProps);
            await _dbContext.SaveChangesAsync();

            // Act
            List<string> recentPasswords = await _passwordRepository.GetRecentPasswordsAsync(userId);

            // Assert
            Assert.Equal(2, recentPasswords.Count);
            Assert.Contains("Password1", recentPasswords);
            Assert.Contains("Password2", recentPasswords);
        }

        [Fact]
        public async Task GetRecentPasswordsAsync_ShouldReturnEmptyList_WhenUserHasNoHistory()
        {
            // Arrange
            ulong userId = 2;
            SysFeatProp sysFeatProps = new SysFeatProp { Name = "History", Val = "3" };

            _dbContext.SysFeatProps.Add(sysFeatProps);
            await _dbContext.SaveChangesAsync();

            // Act
            List<string> recentPasswords = await _passwordRepository.GetRecentPasswordsAsync(userId);

            // Assert
            Assert.Empty(recentPasswords);
        }

        [Fact]
        public async Task GetRecentPasswordsAsync_ShouldReturnEmptyList_WhenSysFeatPropHistoryValIsZeroOrNegative()
        {
            // Arrange
            ulong userId = 1;
            List<PasswordHistory> passwordHistories = new List<PasswordHistory>
        {
            new PasswordHistory { UserId = userId, Password = "Password1", CreatedDate = DateTime.UtcNow.AddDays(-1) },
            new PasswordHistory { UserId = userId, Password = "Password2", CreatedDate = DateTime.UtcNow.AddDays(-2) }
        };

            SysFeatProp sysFeatProps = new SysFeatProp { Name = "History", Val = "0" }; // or a negative value

            _dbContext.PasswordHistories.AddRange(passwordHistories);
            _dbContext.SysFeatProps.Add(sysFeatProps);
            await _dbContext.SaveChangesAsync();

            // Act
            List<string> recentPasswords = await _passwordRepository.GetRecentPasswordsAsync(userId);

            // Assert
            Assert.Empty(recentPasswords);
        }
        [Fact]
        public async Task ForgetPasswordCredentials_ShouldReturnUserCredentials_WhenUserExists()
        {
            // Arrange
            string userName = "testuser";
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = 1,
                UserName = userName,
                Password = "hashedPassword",
                Email = "testuser@example.com",
                Domain = "example.com"
            };

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing ForgetPasswordCredentials for user: {UserName}", userName);

            // Act
            GetForgetPasswordDto result = await _passwordRepository.ForgetPasswordCredentials(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.userId);
            Assert.Equal(user.Password, result.password);
            Assert.Equal(user.Email, result.emailId);
            Assert.Equal(user.Domain, result.domain);
            Assert.Equal(userName, result.userName);

            Log.Information("User credentials retrieved for user {UserName}: {@Result}", userName, result);
        }

        [Fact]
        public async Task ForgetPasswordCredentials_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            string userName = "nonexistentuser";
            Log.Information("Testing ForgetPasswordCredentials for non-existent user: {UserName}", userName);

            // Act
            GetForgetPasswordDto result = await _passwordRepository.ForgetPasswordCredentials(userName);

            // Assert
            Assert.Null(result);
            Log.Information("Expected result: Null returned for user: {UserName}", userName);
        }

        [Fact]
        public async Task UpdateHashedUserNamePassword_ShouldUpdateHashedToken_WhenUserExists()
        {
            // Arrange
            ulong userId = 1ul; // Use ulong
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = userId,
                UserName = "testuser",
                ResetPasswordToken = null,
                ResetPasswordTokenExpiry = null
            };

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Save changes to persist the user

            string newHashedToken = "newHashedToken";

            // Act
            Log.Information("Updating hashed user name password for user ID: {UserId}", userId);
            string result = await _passwordRepository.UpdateHashedUserNamePassword(userId, newHashedToken);

            // Assert
            Assert.Equal(newHashedToken, result); // Check the return value
            Data.SQLCipher.User? updatedUser = await _dbContext.Users.FindAsync(userId);
            Assert.Equal(newHashedToken, updatedUser.ResetPasswordToken); // Check the updated token
            Assert.NotNull(updatedUser.ResetPasswordTokenExpiry); // Ensure timestamp is set

            Log.Information("Successfully updated hashed user name password for user ID: {UserId}", userId);
        }

        [Fact]
        public async Task GetUserNamePasswordFromEmailId_ShouldReturnUserCredentials_WhenEmailExists()
        {
            // Arrange
            string email = "testuser@example.com";
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = 1,
                UserName = "testuser",
                Password = "hashedPassword",
                ResetPasswordToken = "someHashedToken",
                ResetPasswordTokenExpiry = DateTime.UtcNow,
                Email = email
            };

            // Add the user to the in-memory database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing GetUserNamePasswordFromEmailId for email: {Email}", email);

            // Act
            GetUserNamePasswordDto result = await _passwordRepository.GetUserNamePasswordFromEmailId(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserName, result.userName);
            Assert.Equal(user.Password, result.password);
            Assert.Equal(user.ResetPasswordToken, result.hashedUserNamePassword);
            Assert.Equal(user.ResetPasswordTokenExpiry, result.hashedUserNamePasswordTime);

            Log.Information("User credentials retrieved for email {Email}: {@Result}", email, result);
        }

        [Fact]
        public async Task GetUserNamePasswordFromEmailId_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange
            string email = "nonexistentuser@example.com";
            Log.Information("Testing GetUserNamePasswordFromEmailId for non-existent email: {Email}", email);

            // Act
            GetUserNamePasswordDto? result = await _passwordRepository.GetUserNamePasswordFromEmailId(email);

            // Assert
            Assert.Null(result);
            Log.Information("Result for email {Email}: {Result}", email, result);
        }

        [Fact]
        public async Task GetPasswordExpiryWarningValue_ShouldReturnValue_WhenNameExists()
        {
            // Arrange
            string name = "PasswordExpiryWarning";
            SysFeatProp sysFeatProp = new SysFeatProp
            {
                Name = name,
                Val = "WarningValue"
            };

            // Add the SysFeatProp to the in-memory database
            _dbContext.SysFeatProps.Add(sysFeatProp);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing GetPasswordExpiryWarningValue for name: {Name}", name);

            // Act
            string result = await _passwordRepository.GetPasswordExpiryWarningValue(name);

            // Assert
            Assert.Equal(sysFeatProp.Val, result);
            Log.Information("Expected value for name {Name}: {ExpectedValue}, Actual value: {ActualValue}", name, sysFeatProp.Val, result);
        }

        [Fact]
        public async Task GetPasswordExpiryWarningValue_ShouldReturnEmpty_WhenNameDoesNotExist()
        {
            // Arrange
            string name = "NonExistentName";
            Log.Information("Testing GetPasswordExpiryWarningValue for non-existent name: {Name}", name);

            // Act
            string result = await _passwordRepository.GetPasswordExpiryWarningValue(name);

            // Assert
            Assert.Equal(string.Empty, result);
            Log.Information("Result for name {Name}: {Result}", name, result);
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldReturnUserId_WhenTokenExists()
        {
            // Arrange
            string hashedToken = "validHashedToken";
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = 1,
                ResetPasswordToken = hashedToken
            };

            DbContextOptions<Db> options = new DbContextOptionsBuilder<Db>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            using (Db context = new Db(options, string.Empty))
            {
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                context.Users.Add(user);
                await context.SaveChangesAsync();

                PasswordRepository passwordRepository = new PasswordRepository(context);

                Log.Information("Testing CheckForHashedToken for token: {HashedToken}", hashedToken);

                // Act
                ulong result = await passwordRepository.CheckForHashedToken(hashedToken);

                // Assert
                Assert.Equal(user.Id, result);
                Log.Information("Expected user ID for token {HashedToken}: {ExpectedId}, Actual ID: {ActualId}", hashedToken, user.Id, result);
            }
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldReturnZero_WhenTokenDoesNotExist()
        {
            // Arrange
            string hashedToken = "nonExistentToken";
            Log.Information("Testing CheckForHashedToken for non-existent token: {HashedToken}", hashedToken);

            // Act
            ulong result = await _passwordRepository.CheckForHashedToken(hashedToken);

            // Assert
            Assert.Equal(0UL, result); // Ensure the value is treated as a ulong
            Log.Information("Result for hashed token {HashedToken}: {Result}", hashedToken, result);
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldThrowArgumentException_WhenTokenIsNull()
        {
            // Arrange
            string hashedToken = null;
            Log.Information("Testing CheckForHashedToken with null token.");

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.CheckForHashedToken(hashedToken));

            Assert.Equal("Hashed token is required.", exception.Message);
            Log.Warning("Expected exception thrown for null token: {Message}", exception.Message);
        }

        [Fact]
        public async Task CheckForHashedToken_ShouldThrowArgumentException_WhenTokenIsEmpty()
        {
            // Arrange
            string hashedToken = "";
            Log.Information("Testing CheckForHashedToken with empty token.");

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _passwordRepository.CheckForHashedToken(hashedToken));

            Assert.Equal("Hashed token is required.", exception.Message);
            Log.Warning("Expected exception thrown for empty token: {Message}", exception.Message);
        }

        [Fact]
        public async Task CheckForHashedTokenWithUserDetails_ShouldReturnUserCredentialsDto_WhenTokenIsValid()
        {
            // Arrange
            string hashedToken = "validToken";
            Data.SQLCipher.User user = new Data.SQLCipher.User
            {
                Id = 1,
                UserName = "testuser",
                Password = "hashedPassword",
                ResetPasswordToken = hashedToken
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Testing CheckForHashedTokenWithUserDetails for token: {HashedToken}", hashedToken);

            // Act
            UserCredentialsDto result = await _passwordRepository.CheckForHashedTokenWithUserDetails(hashedToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.userId);
            Assert.Equal(user.Password, result.Password);

            Log.Information("Result for token {HashedToken}: UserId: {UserId}, Password: {Password}", hashedToken, result.userId, result.Password);
        }

        [Fact]
        public async Task CheckForHashedTokenWithUserDetails_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Arrange
            string hashedToken = "nonExistentToken";
            Log.Information("Testing CheckForHashedTokenWithUserDetails for non-existent token: {HashedToken}", hashedToken);

            // Act
            UserCredentialsDto? result = await _passwordRepository.CheckForHashedTokenWithUserDetails(hashedToken);

            // Assert
            Assert.Null(result);
            Log.Information("Result for hashed token {HashedToken}: {Result}", hashedToken, result);
        }
    }
}
