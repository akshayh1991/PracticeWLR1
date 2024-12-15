using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using SecMan.Data.DAL;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Model;
using SecMan.Model.Common;
using SecMan.UnitTests.FaultyDbConfig;
using SecMan.UnitTests.Logger;
using Serilog;


namespace SecMan.UnitTests.DAL;
[CustomLogging]
[Collection("Sequential Collection")]
public class UserRepositoryTests : IDisposable
{
    private readonly DbContextOptions<Db> _dbContextOptions;
    private readonly Db _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly IFixture _fixture;
    private readonly Mock<IHttpContextAccessor> _mockHttpContext;


    public UserRepositoryTests()
    {
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbContextOptions = new DbContextOptionsBuilder<Db>()
        .UseSqlite("DataSource=:memory:")
        .Options;

        _db = new Db(_dbContextOptions, string.Empty);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        Data.SQLCipher.SysFeatProp sysFeatProp = _fixture.Build<Data.SQLCipher.SysFeatProp>()
                        .With(x => x.Name, "Expiry")
                        .With(x => x.Val, "30")
                        .Create();
        Data.SQLCipher.SysFeat sysFeat = _fixture.Build<Data.SQLCipher.SysFeat>()
            .With(x => x.SysFeatProps, new List<SysFeatProp> { sysFeatProp })
            .With(x => x.Id, 3ul)
            .Create();
        _db.AddRange(sysFeat);
        List<Data.SQLCipher.Role> roles = new List<Data.SQLCipher.Role>
        {
                new Data.SQLCipher.Role { Id = 1, Name = "Admin", Description = "Administrator role", IsLoggedOutType = false },
                new Data.SQLCipher.Role { Id = 2, Name = "User", Description = "User role", IsLoggedOutType = false }
        };

        _db.Roles.AddRange(roles);
        _db.Users.AddRange(
                new Data.SQLCipher.User { Id = 1, UserName = "Admin", Description = "Administrator role", Roles = new List<Data.SQLCipher.Role> { roles[0] }, Retired = false },
                new Data.SQLCipher.User { Id = 2, UserName = "User", Description = "User role", Roles = new List<Data.SQLCipher.Role> { roles[0] } }
            );
        _db.SysFeatProps.AddRange(new SysFeatProp
        {
            Name = "Max Login Attempts",
            ValMax = 3ul
        });

        _db.SaveChanges();
        _mockHttpContext = new Mock<IHttpContextAccessor>();
        _mockConfiguration = new Mock<IConfiguration>();
        _unitOfWork = new UnitOfWork(_db, _mockHttpContext.Object, _mockConfiguration.Object);

    }


    // AddUserAsync
    [Fact]
    public async Task AddUserAsync_ShouldReturnInsertedUserId()
    {
        // Arrange
        CreateUser addUserDto = _fixture.Build<CreateUser>()
            .With(x => x.IsPasswordExpiryEnabled, true)
            .Create();
        addUserDto.Roles = [1, 2];
        int usersCount = (await _unitOfWork.IUserRepository.GetAll()).Count();

        Log.Information("Completed Moqing dependencies");

        // Act
        Model.User result = await _unitOfWork.IUserRepository.AddUserAsync(addUserDto);
        await _unitOfWork.SaveChangesAsync();
        int usersCountAfterInsert = (await _unitOfWork.IUserRepository.GetAll()).Count();
        Log.Information("Test result: {@Result}", result);

        // Assert
        Assert.Equal(usersCount + 1, usersCountAfterInsert);
        Log.Information("Verified UserId: {@userId} is not equal to 0", result.Id);
        Assert.NotNull(result);
        Log.Information("Verified result: {@Result} is not null", result);
        Assert.Equal(result.Roles.Count, addUserDto.Roles.Count);
        Log.Information("Verified if all the roles : {@roles} are added and mapped to user", result);
    }

    [Fact]
    public async Task AddUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        CreateUser addUserDto = _fixture.Create<CreateUser>();
        addUserDto.Roles = _fixture.CreateMany<ulong>(2).ToList();

        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));
        Log.Information("Completed Moqing dependencies");

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.AddUserAsync(addUserDto));
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }


    [Fact]
    public async Task AddUserAsync_ShouldThrowInternalServerError_WhenNullReferenceExceptionOccurs()
    {
        // Arrange
        CreateUser? addUserDto = null;


        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _unitOfWork.IUserRepository.AddUserAsync(addUserDto));
        Log.Information("Verified if the method throwing NullReferenceException for null input value");
    }



    // UpdateUserAsync
    [Fact]
    public async Task UpdateUserAsync_ShouldReturnUpdatedUserId()
    {
        // Arrange
        CreateUser addUserDto = _fixture.Create<CreateUser>();
        Model.UpdateUser updateUserDto = _fixture.Create<Model.UpdateUser>();
        addUserDto.Roles = [1, 2];
        updateUserDto.Roles = addUserDto.Roles;
        Log.Information("Completed Moqing dependencies");

        // Act
        Model.User user = await _unitOfWork.IUserRepository.AddUserAsync(addUserDto);
        await _unitOfWork.SaveChangesAsync();
        Data.SQLCipher.User? insertedUser = await _unitOfWork.IUserRepository.GetUserByUsername(addUserDto.Username);
        Model.User updatedUser = await _unitOfWork.IUserRepository.UpdateUserAsync(updateUserDto, insertedUser.Id);
        Log.Information("Test result: {@Result}", updatedUser);

        // Assert
        Assert.NotNull(updatedUser);
        Log.Information("Verified if the updated user object : {@updatedUser} is not null", updatedUser);
        Assert.Equal(insertedUser.Id, updatedUser.Id);
        Log.Information("Verified if the user id passed and updated user object's id are same({@userid}={@updatedUserId})", user.Id, updatedUser.Id);
        Assert.Equal(updatedUser.Roles.Count, updateUserDto.Roles.Count);
        Log.Information("Verified if all the roles : {@roles} are added and mapped to user", updateUserDto.Roles);
    }


    [Fact]
    public async Task UpdateUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        CreateUser addUserDto = _fixture.Create<CreateUser>();
        Model.User updateUserDto = _fixture.Create<Model.User>();
        addUserDto.Roles = _fixture.CreateMany<ulong>(2).ToList();

        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));

        Log.Information("Completed Moqing dependencies");

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.UpdateUserAsync(updateUserDto, updateUserDto.Id));
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }


    [Fact]
    public async Task UpdateUserAsync_ShouldThrowInternalServerError_WhenArgumentNullExceptionOccurs()
    {
        // Arrange
        Model.User? updateUserDto = _fixture.Create<Model.User>();
        ulong userId = 3ul;
        Log.Information("Completed Moqing dependencies");


        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _unitOfWork.IUserRepository.UpdateUserAsync(updateUserDto, userId));
        Log.Information("Verified if the method throwing ArgumentNullException if the updated requested userId : {@userId} doesnot exists", userId);
    }


    [Fact]
    public async Task UpdateUserAsync_ShouldThrowInternalServerError_WhenNullReferenceExceptionOccurs()
    {
        // Arrange
        Model.User? updateUserDto = null;
        ulong userId = 1;
        Log.Information("Completed Moqing dependencies");

        // Act

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _unitOfWork.IUserRepository.UpdateUserAsync(updateUserDto, userId));
        Log.Information("Verified if the method throwing NullReferenceException for null input value");
    }



    // GetRolesByRoleId
    [Fact]
    public async Task GetRolesByRoleId_ReturnsRoles_WhenRoleIdsAreValid()
    {
        // Arrange
        List<ulong> roleIds = new List<ulong> { 1 };

        // Act
        List<Model.RoleModel> roles = await _unitOfWork.IUserRepository.GetRolesByRoleId(roleIds);
        Log.Information("Test result: {@Result}", roles);


        // Assert
        Assert.Single(roles);
        Assert.Equal("Admin", roles.First().Name);
        Log.Information("Verified if the returned role objects Name matchs with input role id related name");
    }


    [Fact]
    public async Task GetRolesByRoleId_ReturnsEmpty_WhenNoRoleIdsMatch()
    {
        // Arrange
        List<ulong> roleIds = new List<ulong> { 999 };

        // Act
        List<Model.RoleModel> roles = await _unitOfWork.IUserRepository.GetRolesByRoleId(roleIds);
        Log.Information("Test result: {@Result}", roles);


        // Assert
        Assert.Empty(roles);
        Log.Information("Verified if the roles are empty if id passed in filter is invalid");
    }


    [Fact]
    public async Task GetRolesByRoleId_GetRolesByRoleId_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        List<ulong> roleIds = new List<ulong> { 1 };
        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));
        Log.Information("Completed Moqing dependencies");

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetRolesByRoleId(roleIds));
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }


    // GetUserByUsername
    [Fact]
    public async Task GetUserByUsername_ShouldReturn_MatchingUser()
    {
        // Arrange
        string username = "Admin";

        // Act
        Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetUserByUsername(username);
        Log.Information("Test result: {@Result}", user);


        // Assert
        Assert.NotNull(user);
        Log.Information("Verified if the returned user is not null");
        Assert.Equal(username, user.UserName);
        Log.Information("Verified if the returened user object's username matchs with input username");
    }


    [Fact]
    public async Task GetUserByUsername_ShouldReturn_Null()
    {
        // Arrange
        string username = "test user";

        // Act
        Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetUserByUsername(username);
        Log.Information("Test result: {@Result}", user);


        // Assert
        Assert.Null(user);
        Log.Information("Verified if the returned user object is null for invalid username");
    }


    [Fact]
    public async Task GetUserByUsername_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        string username = "test user";
        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));
        Log.Information("Completed Moqing dependencies");

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetUserByUsername(username));
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }



    // GetUserById
    [Fact]
    public async Task GetUserById_ShouldReturn_MatchingUser()
    {
        // Arrange
        ulong userId = 1ul;

        // Act
        Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetById(userId);
        Log.Information("Test result: {@Result}", user);

        // Assert
        Assert.NotNull(user);
        Log.Information("Verified if the returned user is not null");
        Assert.Equal(userId, user.Id);
        Log.Information("Verified if the returened user object's id matchs with input id");
    }


    [Fact]
    public async Task GetUserById_ShouldReturn_Null()
    {
        // Arrange
        ulong userId = 100ul;

        // Act
        Data.SQLCipher.User? user = await _unitOfWork.IUserRepository.GetById(userId);
        Log.Information("Test result: {@Result}", user);

        // Assert
        Assert.Null(user);
        Log.Information("Verified if the returned user object is null for invalid id");
    }


    [Fact]
    public async Task GetUserById_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        ulong userId = 1ul;
        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));
        Log.Information("Completed Moqing dependencies");

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetUserById(userId));
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }



    // GetUsers
    [Fact]
    public async Task GetUsers_ReturnsAllUsers_AsUserDto()
    {
        // Arrange

        // Act
        List<Data.SQLCipher.User> users = (await _unitOfWork.IUserRepository.GetAll()).ToList();
        Log.Information("Test result: {@Result}", users);

        // Assert
        Assert.Equal(2, users.Count);
        Log.Information("Verified if user count returned matchs with seeded user count: {@userCount}", users.Count);
        Assert.Contains(users, r => r.Id == 1 && r.UserName == "Admin");
        Log.Information("Verified the first user is Admin");
        Assert.Contains(users, r => r.Id == 2 && r.UserName == "User");
        Log.Information("Verified the last user is User");
    }


    [Fact]
    public async Task GetUsers_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));
        Log.Information("Completed Moqing dependencies");


        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetAll());
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }


    [Fact]
    public async Task DeleteUserAsync_ShouldRemove_UserFromDb()
    {
        // Arrange
        ulong userId = 1ul;

        // Act
        await _unitOfWork.IUserRepository.Delete(userId);
        await _unitOfWork.SaveChangesAsync();
        IEnumerable<Data.SQLCipher.User> users = await _unitOfWork.IUserRepository.GetAll();
        Data.SQLCipher.User? deletedUser = users.Where(x => x.Id == userId).FirstOrDefault();
        Log.Information("Test result: {@Result}", deletedUser);


        // Assert
        Assert.DoesNotContain(users, x => x.Id == userId);
        Log.Information("Verified if the users object : {@users} does not contain deleted user", users);
        Assert.Null(deletedUser);
        Log.Information("Verified if the object is null when deleted user id is queried");
    }


    [Fact]
    public async Task DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
    {
        // Arrange
        ulong userId = 1ul;
        UserRepository userWithFaultyDb = new UserRepository(new FaultyDbContext(_dbContextOptions));
        Log.Information("Completed Moqing dependencies");

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.Delete(userId));
        Log.Information("Verified if the method throwing SqliteException for any db related issues");
    }


    [Fact]
    public async Task LogLoginAttempts_ShouldReturn_CompiledTask_IfUserDoesNotExists()
    {
        // Arrange
        ulong userId = 100;
        bool isSuccess = true;

        // Act
        await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, isSuccess);
        List<EventLogs> logs = await _db.EventLogs.Where(x => x.User.Id == userId && x.EventType == EventType.User && x.EventSubType == EventSubType.Login).ToListAsync();
        Log.Information("Test result: {@Result}", logs);

        // Assert
        Assert.Empty(logs);
        Log.Information("Verified If the Login Logs are empty");
    }


    [Fact]
    public async Task LogLoginAttempts_ShouldReturn_CompledTask_WithZero_Logs_SinceIts_SuccessLogin()
    {
        // Arrange
        ulong userId = 1;
        bool isSuccess = true;

        // Act
        await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, isSuccess);
        List<EventLogs> logs = await _db.EventLogs.Where(x => x.User.Id == userId && x.EventType == EventType.User && x.EventSubType == EventSubType.Login).ToListAsync();
        Log.Information("Test result: {@Result}", logs);

        // Assert
        Assert.False(logs.TrueForAll(x => x.Message == EventLogConstants.LoginFailed));
        Log.Information("Verified If the Login Logs are empty");
    }


    [Fact]
    public async Task LogLoginAttempts_ShouldReturn_CompledTask_WithInserting_LogInto_DB()
    {
        // Arrange
        ulong userId = 1;
        bool isSuccess = false;

        // Act
        await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, isSuccess);
        List<EventLogs> logs = await _db.EventLogs.Where(x => x.User.Id == userId && x.EventType == EventType.User && x.EventSubType == EventSubType.Login)
            .Include(x => x.User).ToListAsync();
        Log.Information("Test result: {@Result}", logs);

        // Assert
        Assert.Single(logs);
        Log.Information("Verified if only one log in added");
        Assert.False(logs.Select(x => x.User.Locked).FirstOrDefault());
        Log.Information("Verified if the user is not locked since this was an 1st attempt");
    }


    [Fact]
    public async Task LogLoginAttempts_ShouldReturn_CompledTask_ByLockingUser()
    {
        // Arrange
        ulong userId = 1;
        bool isSuccess = false;
        await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, isSuccess);
        await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, isSuccess);
        Log.Information("Completed Moqing dependencies");

        // Act
        await _unitOfWork.IEventLogRepository.LogLoginAttempts(userId, isSuccess);
        List<EventLogs> logs = await _db.EventLogs.Where(x => x.User.Id == userId && x.EventType == EventType.User && x.EventSubType == EventSubType.Login)
            .Include(x => x.User).ToListAsync();
        Log.Information("Test result: {@Result}", logs);


        // Assert
        Assert.Equal(3, logs.Count);
        Log.Information("verified there a total 3 logs for the same user");
        Assert.True(logs.Select(x => x.User.Locked).FirstOrDefault());
        Log.Information("verified that the user is locked since 3 max wrong attempts allowed");
    }



    [Fact]
    public async Task GetUserBySessionId_ShouldReturn_Null_ForInvalidSessionId()
    {
        // Arrange
        string sessionId = _fixture.Create<string>();

        // Act
        ulong? result = await _unitOfWork.IUserRepository.GetUserBySessionId(sessionId);
        Log.Information("Test result: {@Result}", result);

        // Assert
        Assert.Equal(0ul, result);
        Log.Information("Verified if the response is null for invalid session id");
    }


    [Fact]
    public async Task GetUserBySessionId_ShouldReturn_UserId_ForValidSessionId()
    {
        // Arrange
        ulong userId = 1;
        string sessionId = _fixture.Create<string>();
        await _unitOfWork.IUserRepository.UpdateUserSessionDetails(userId, sessionId, 5);
        Log.Information("Completed Moqing dependencies");

        // Act
        ulong? result = await _unitOfWork.IUserRepository.GetUserBySessionId(sessionId);
        Log.Information("Test result: {@Result}", result);

        // Assert
        Assert.NotNull(result);
        Log.Information("verified that the user details are not null since user id is valid");
    }


    [Fact]
    public async Task GetUserDetails_ShouldReturn_DefaultValues_IfUserId_IsInvalid()
    {
        // Arrange
        ulong userId = 100;

        // Act
        Tuple<UserDetails?, List<AppPermissions>?> result = await _unitOfWork.IUserRepository.GetUserDetails(userId);
        Log.Information("Test result: {@Result}", result);

        // Assert
        Assert.Equal(default, result.Item1);
        Log.Information("Verified if the response of the user details are null or default for invalid user id");
        Assert.Equal(default, result.Item2);
        Log.Information("Verified if the response of the permissions are null or default for invalid user id");
    }


    [Fact]
    public async Task GetUserDetails_ShouldReturn_UserDetails_IfUserId_IsValid()
    {
        // Arrange
        ulong userId = 1;

        // Act
        Tuple<UserDetails?, List<AppPermissions>?> result = await _unitOfWork.IUserRepository.GetUserDetails(userId);
        Log.Information("Test result: {@Result}", result);

        // Assert
        Assert.NotEqual(default, result.Item1);
        Log.Information("Verified if the response of the user details are not null or default for valid user id");
        Assert.NotEqual(default, result.Item2);
        Log.Information("Verified if the response of the permissions are not null or default for valid user id");
        Assert.Single(result.Item1.Roles);
        Log.Information("Verified if the roles assigned to the user is 1");
    }


    // RetireUserAsync
    [Fact]
    public async Task RetireUserAsync_Should_RetireUser()
    {
        // Arrange
        ulong userId = 1ul;
        Data.SQLCipher.User user = await _unitOfWork.IUserRepository.GetById(userId);

        // Act
        await _unitOfWork.IUserRepository.RetireUserAsync(user);
        Data.SQLCipher.User result = await _unitOfWork.IUserRepository.GetById(userId);
        Log.Information("Test result: {@Result}", result);

        // Assert
        Assert.True(result.Retired);
        Log.Information("Verified if the user is retired successfully");
        Assert.NotEqual(DateTime.MinValue, result.RetiredDate);
        Log.Information("Verified if retired date is updated");
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    // UnlockUserAsync
    [Fact]
    public async Task UnlockUserAsync_ShouldUnlockUser_WhenUserIsLocked()
    {
        // Arrange
        Data.SQLCipher.User user = _db.Users.First(u => u.Id == 1);
        UserRepository repository = new UserRepository(_db);

        // Act
        await repository.UnlockUserAsync(user, changePasswordOnLogin: true);
        _db.SaveChanges();

        // Assert
        Assert.False(user.Locked);
        Assert.Equal(DateTime.MinValue, user.LockedDate);
        Assert.True(user.FirstLogin);
    }

    [Fact]
    public async Task UnlockUserAsync_ShouldNotChangeUser_WhenUserIsAlreadyUnlocked()
    {
        // Arrange
        ulong userId = (ulong)2;
        Data.SQLCipher.User user = await _unitOfWork.IUserRepository.GetById(userId);
        bool changePasswordOnLogin = false;

        // Act
        await _unitOfWork.IUserRepository.UnlockUserAsync(user, changePasswordOnLogin);

        // Assert
        Data.SQLCipher.User unchangedUser = await _unitOfWork.IUserRepository.GetById(userId);
        Assert.False(unchangedUser.Locked);
        Assert.Equal(DateTime.MinValue, unchangedUser.LockedDate);
    }

    // UpdateUserLanguageAsync
    [Fact]
    public async Task UpdateUserLanguageAsync_ShouldReturnTrue_WhenUserExists()
    {
        ulong userId = 1L;
        Data.SQLCipher.User user = new Data.SQLCipher.User
        {
            Id = userId,
            UserName = "testuser",
            Language = "en"
        };
        Log.Information("Clearing existing users from the database.");
        _db.Users.RemoveRange(_db.Users);
        await _db.SaveChangesAsync();
        Log.Information("Adding a new user with username: {UserName}, initial language: {Language}", user.UserName, user.Language);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        Log.Information("Updating language for user: {UserName}", user.UserName);
        bool result = await _unitOfWork.IUserRepository.UpdateUserLanguageAsync(userId, "fr");
        Assert.True(result);
        Data.SQLCipher.User? updatedUser = await _db.Users.FindAsync(userId);
        Assert.Equal("fr", updatedUser?.Language);
        Log.Information("User language successfully updated for username: {UserName}", user.UserName);
    }


    [Fact]
    public async Task UpdateUserLanguageAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        ulong nonExistentUserId = 999;
        Log.Information("Attempting to update language for non-existent user with ID: {UserId}", nonExistentUserId);
        bool result = await _unitOfWork.IUserRepository.UpdateUserLanguageAsync(nonExistentUserId, "fr");
        Assert.False(result);
        Log.Information("Test passed: UpdateUserLanguageAsync returned false for non-existent user ID: {UserId}", nonExistentUserId);
    }

}
