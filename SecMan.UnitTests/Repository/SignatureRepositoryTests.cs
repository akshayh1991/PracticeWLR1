using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.Model.Common;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;
using User = SecMan.Data.SQLCipher.User;

namespace SecMan.UnitTests.Repository
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class SignatureRepositoryTests : IDisposable
    {
        private readonly SignatureRepository _signatureRepository;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Db _dbContext;

        public SignatureRepositoryTests()
        {
            // Setup in-memory database for testing
            DbContextOptions<Db> options = new DbContextOptionsBuilder<Db>()
                .UseInMemoryDatabase("TestSignatureDatabase")
                .Options;

            _dbContext = new Db(options, string.Empty);
            _dbContext.Database.EnsureCreated();

            // Initialize the SignatureRepository with the in-memory context
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _signatureRepository = new SignatureRepository(_dbContext, _httpContextAccessorMock.Object);
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
            string userName = "testUser";
            string userPassword = "2$hashedPassword";

            Log.Information("Seeding the database with a test user: {UserName}", userName);
            _dbContext.Users.Add(new User { Id = 1, UserName = userName, Password = userPassword });
            await _dbContext.SaveChangesAsync(); // Use await for async save
            Log.Information("Test user seeded successfully.");

            // Act
            Log.Information("Calling GetUserCredentials with userName: {UserName}", userName);
            GetUserCredentialsDto result = await _signatureRepository.GetUserCredentials(userName);

            // Assert
            Assert.NotNull(result);
            Log.Information("Asserted that result is not null.");

            Assert.Equal((ulong)1, result.userId); // Cast to ulong for comparison
            Log.Information("Asserted result.userId equals 1.");

            Assert.Equal(userPassword, result.Password);
            Log.Information("Asserted result.Password matches the expected password.");
        }

        [Fact]
        public async Task GetUserCredentials_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            string userName = "nonExistentUser";
            Log.Information("Calling GetUserCredentials for a non-existent user: {UserName}", userName);

            // Act
            GetUserCredentialsDto result = await _signatureRepository.GetUserCredentials(userName);

            // Assert
            Assert.Null(result);
            Log.Information("Asserted that result is null for a non-existent user.");
        }

        #region Verify Signature

        [Fact]
        public async Task SignatureVerifyAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            ulong userId = 1;
            string userNote = "This is a test note";

            Log.Information("Seeding the database with a test user: {UserId}", userId);
            _dbContext.Users.Add(new User { Id = userId, UserName = "testUser", Password = "hashedPassword" });
            await _dbContext.SaveChangesAsync();
            Log.Information("Test user seeded successfully.");

            // Act
            Log.Information("Calling SignatureVerifyAsync with userId: {UserId}, note: {Note}", userId, userNote);
            bool result = await _signatureRepository.SignatureVerifyAsync(userId, userNote);

            // Assert
            Assert.True(result);
            Log.Information("Asserted that result is true.");

            Log.Information("SignatureVerifyAsync completed successfully for userId: {UserId}, note: {Note}", userId, userNote);
        }

        [Fact]
        public async Task SignatureVerifyAsync_ValidUserId_CreatesEventLogAndReturnsTrue()
        {
            // Arrange
            ulong userId = 1UL;
            string note = "Test note";
            User user = new User { Id = userId };

            Log.Information("Seeding the database with a test user: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Explicit save
            Log.Information("Test user seeded successfully.");

            Mock<ConnectionInfo> connectionMock = new Mock<ConnectionInfo>();
            connectionMock.SetupGet(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContextMock.Object);
            Log.Information("HttpContext mock setup complete with remote IP address: {IpAddress}", "127.0.0.1");

            // Act
            Log.Information("Calling SignatureVerifyAsync with userId: {UserId}, note: {Note}", userId, note);
            bool result = await _signatureRepository.SignatureVerifyAsync(userId, note);

            // Assert
            Assert.True(result);
            Log.Information("Asserted that result is true.");

            await _dbContext.SaveChangesAsync(); // Ensure any changes are persisted
            EventLogs? eventLog = _dbContext.EventLogs.FirstOrDefault(e => e.User.Id == userId && e.Message == note);
            Assert.NotNull(eventLog);
            Log.Information("Asserted that eventLog is not null for userId: {UserId}, note: {Note}", userId, note);

            Assert.Equal(EventType.Signature, eventLog.EventType);
            Log.Information("Asserted that eventLog.EventType is Signature.");

            Assert.Equal(EventSubType.Verify, eventLog.EventSubType);
            Log.Information("Asserted that eventLog.EventSubType is Verify.");

            Assert.Equal(EventSeverity.Info, eventLog.Severity);
            Log.Information("Asserted that eventLog.Severity is Info.");

            Assert.Equal("127.0.0.1", eventLog.Source);
            Log.Information("Asserted that eventLog.Source is {IpAddress}.", "127.0.0.1");
        }

        [Fact]
        public async Task SignatureVerifyAsync_UserNotFound_CreatesEventLogWithNullUserAndReturnsTrue()
        {
            // Arrange
            ulong userId = 999UL;
            string note = "Test note";

            Mock<ConnectionInfo> connectionMock = new Mock<ConnectionInfo>();
            connectionMock.SetupGet(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContextMock.Object);
            Log.Information("HttpContext mock setup complete with remote IP address: {IpAddress}", "127.0.0.1");

            // Act
            Log.Information("Calling SignatureVerifyAsync with non-existent userId: {UserId}, note: {Note}", userId, note);
            bool result = await _signatureRepository.SignatureVerifyAsync(userId, note);

            // Assert
            Assert.True(result);
            Log.Information("Asserted that result is true.");

            await _dbContext.SaveChangesAsync(); // Ensure changes are persisted
            EventLogs? eventLog = _dbContext.EventLogs.FirstOrDefault(e => e.User == null && e.Message == note);
            Assert.NotNull(eventLog);
            Log.Information("Asserted that eventLog is not null for a null user and note: {Note}", note);

            Assert.Equal("127.0.0.1", eventLog.Source);
            Log.Information("Asserted that eventLog.Source is {IpAddress}.", "127.0.0.1");
        }


        [Fact]
        public async Task SignatureVerifyAsync_NullHttpContextSource_CreatesEventLogWithNullSource()
        {
            // Arrange
            ulong userId = 1UL;
            string note = "Test note";
            User user = new User { Id = userId };

            Log.Information("Seeding database with user: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Explicit save

            Log.Information("Setting up HttpContextAccessor with null HttpContext");
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns((HttpContext)null);

            // Act
            Log.Information("Calling SignatureVerifyAsync for userId: {UserId}, note: {Note}", userId, note);
            bool result = await _signatureRepository.SignatureVerifyAsync(userId, note);

            // Assert
            Log.Information("Asserting result for SignatureVerifyAsync");
            Assert.True(result);

            Log.Information("Saving changes to database");
            await _dbContext.SaveChangesAsync(); // Ensure changes are persisted

            EventLogs? eventLog = _dbContext.EventLogs.FirstOrDefault(e => e.User.Id == userId && e.Message == note);
            Log.Information("Checking created event log for userId: {UserId}, note: {Note}", userId, note);
            Assert.NotNull(eventLog);
            Assert.Null(eventLog.Source);
        }

        [Fact]
        public async Task SignatureVerifyAsync_NullNote_CreatesEventLogWithNullMessage()
        {
            // Arrange
            ulong userId = 1UL;
            string note = null; // Null note
            User user = new User { Id = userId };

            Log.Information("Seeding database with user: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Explicit save

            Log.Information("Setting up HttpContextAccessor with RemoteIpAddress");
            Mock<ConnectionInfo> connectionMock = new Mock<ConnectionInfo>();
            connectionMock.SetupGet(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContextMock.Object);

            // Act
            Log.Information("Calling SignatureVerifyAsync for userId: {UserId}, note: {Note}", userId, note);
            bool result = await _signatureRepository.SignatureVerifyAsync(userId, note);

            // Assert
            Log.Information("Asserting result for SignatureVerifyAsync");
            Assert.True(result);

            Log.Information("Saving changes to database");
            await _dbContext.SaveChangesAsync(); // Ensure changes are persisted

            EventLogs? eventLog = _dbContext.EventLogs.FirstOrDefault(e => e.User.Id == userId && e.Message == null);
            Log.Information("Checking created event log for userId: {UserId} with null message", userId);
            Assert.NotNull(eventLog);
            Assert.Equal(EventType.Signature, eventLog.EventType);
            Assert.Equal(EventSubType.Verify, eventLog.EventSubType);
            Assert.Equal(EventSeverity.Info, eventLog.Severity);
            Assert.Equal("127.0.0.1", eventLog.Source);
        }
        #endregion

        #region Authorize Signature
        [Fact]
        public async Task SignatureAuthorizeAsync_UserNotFound_ShouldNotCreateEventLog()
        {
            // Arrange
            ulong userId = 1UL;
            Authorize request = new Authorize { IsNote = false };

            // Act
            Log.Information("Calling SignatureAuthorizeAsync for non-existent userId: {UserId}", userId);
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            // Assert
            EventLogs? eventLog = _dbContext.EventLogs.SingleOrDefault();
            Log.Information("Checking for non-existent event log for userId: {UserId}", userId);
            Assert.Null(eventLog);
        }

        [Fact]
        public async Task SignatureAuthorizeAsync_UserFound_ShouldCreateEventLog()
        {
            // Arrange
            ulong userId = 1;
            Authorize request = new Authorize { IsNote = false };
            User user = new User { Id = userId };

            Log.Information("Seeding database with user: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Setting up HttpContextAccessor with RemoteIpAddress");
            _httpContextAccessorMock.Setup(h => h.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("127.0.0.1"));

            // Act
            Log.Information("Calling SignatureAuthorizeAsync for userId: {UserId}", userId);
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            Log.Information("Saving changes to database");
            await _dbContext.SaveChangesAsync(); // Explicit save

            // Assert
            EventLogs? eventLog = await _dbContext.EventLogs.SingleOrDefaultAsync();
            Log.Information("Checking created event log for userId: {UserId}", userId);
            Assert.NotNull(eventLog);
            Assert.Equal(EventType.Signature, eventLog.EventType);
            Assert.Equal(EventSubType.Authorize, eventLog.EventSubType);
            Assert.Equal(EventSeverity.Info, eventLog.Severity);
            Assert.Equal(user, eventLog.User);
            Assert.Equal("127.0.0.1", eventLog.Source);
            Assert.Null(eventLog.Message);
        }

        [Fact]
        public async Task SignatureAuthorizeAsync_WithNote_ShouldSetMessage()
        {
            // Arrange
            ulong userId = 1;
            Authorize request = new Authorize { IsNote = true, Note = "Test Note" };
            User user = new User { Id = userId };

            Log.Information("Seeding database with user: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            Log.Information("Setting up HttpContextAccessor with RemoteIpAddress");
            _httpContextAccessorMock.Setup(h => h.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("127.0.0.1"));

            // Act
            Log.Information("Calling SignatureAuthorizeAsync for userId: {UserId}, with note: {Note}", userId, request.Note);
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            Log.Information("Saving changes to database");
            await _dbContext.SaveChangesAsync(); // Explicit save

            // Assert
            EventLogs? eventLog = await _dbContext.EventLogs.SingleOrDefaultAsync();
            Log.Information("Checking created event log for userId: {UserId} with message: {Message}", userId, request.Note);
            Assert.NotNull(eventLog);
            Assert.Equal("Test Note", eventLog.Message);
        }
        #endregion


        [Fact]
        public async Task SignatureAuthorizeAsync_WithEmptyNote_ShouldNotSetMessage()
        {
            // Arrange
            ulong userId = 1;
            Authorize request = new Authorize { IsNote = true, Note = string.Empty };
            User user = new User { Id = userId };

            Log.Information("Adding user with ID: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _httpContextAccessorMock.Setup(h => h.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("127.0.0.1"));

            Log.Information("Setup HttpContext with IP: 127.0.0.1");

            // Act
            Log.Information("Calling SignatureAuthorizeAsync with empty note");
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            // Save changes explicitly since the repository does not call SaveChanges
            Log.Information("Saving changes to the database");
            await _dbContext.SaveChangesAsync();

            // Assert
            EventLogs? eventLog = await _dbContext.EventLogs.SingleOrDefaultAsync();
            Log.Information("Asserting the event log");
            Assert.NotNull(eventLog);
            Assert.Null(eventLog.Message);
        }

        [Fact]
        public async Task SignatureAuthorizeAsync_WithoutNote_ShouldNotSetMessage()
        {
            // Arrange
            ulong userId = 1;
            Authorize request = new Authorize { IsNote = false };
            User user = new User { Id = userId };

            Log.Information("Adding user with ID: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _httpContextAccessorMock.Setup(h => h.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("127.0.0.1"));

            Log.Information("Setup HttpContext with IP: 127.0.0.1");

            // Act
            Log.Information("Calling SignatureAuthorizeAsync without note");
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            // Save changes explicitly since the repository does not call SaveChanges
            Log.Information("Saving changes to the database");
            await _dbContext.SaveChangesAsync();

            // Assert
            EventLogs? eventLog = await _dbContext.EventLogs.SingleOrDefaultAsync();
            Log.Information("Asserting the event log");
            Assert.NotNull(eventLog);
            Assert.Null(eventLog.Message);
        }

        [Fact]
        public async Task SignatureAuthorizeAsync_ShouldUseRemoteIpAddress()
        {
            // Arrange
            ulong userId = 1;
            Authorize request = new Authorize { IsNote = false };
            User user = new User { Id = userId };

            Log.Information("Adding user with ID: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _httpContextAccessorMock.Setup(h => h.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("192.168.1.1"));

            Log.Information("Setup HttpContext with IP: 192.168.1.1");

            // Act
            Log.Information("Calling SignatureAuthorizeAsync to verify IP address usage");
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            // Save changes explicitly since the repository does not call SaveChanges
            Log.Information("Saving changes to the database");
            await _dbContext.SaveChangesAsync();

            // Assert
            EventLogs? eventLog = await _dbContext.EventLogs.SingleOrDefaultAsync();
            Log.Information("Asserting the event log and verifying IP");
            Assert.NotNull(eventLog);
            Assert.Equal("192.168.1.1", eventLog.Source);
        }

        [Fact]
        public async Task SignatureAuthorizeAsync_ShouldUseUtcNowForDate()
        {
            // Arrange
            ulong userId = 1;
            Authorize request = new Authorize { IsNote = false };
            User user = new User { Id = userId };

            Log.Information("Adding user with ID: {UserId}", userId);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _httpContextAccessorMock.Setup(h => h.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("127.0.0.1"));

            Log.Information("Setup HttpContext with IP: 127.0.0.1");

            DateTime expectedUtcNow = DateTime.UtcNow;

            // Act
            Log.Information("Calling SignatureAuthorizeAsync to verify timestamp");
            await _signatureRepository.SignatureAuthorizeAsync(userId, userId, request);

            // Save changes explicitly since the repository does not call SaveChanges
            Log.Information("Saving changes to the database");
            await _dbContext.SaveChangesAsync();

            // Assert
            EventLogs? eventLog = await _dbContext.EventLogs.SingleOrDefaultAsync();
            Log.Information("Asserting the event log and verifying timestamp");
            Assert.NotNull(eventLog);
            Assert.InRange(eventLog.Date, expectedUtcNow, expectedUtcNow.AddSeconds(1));
        }



    }
}
