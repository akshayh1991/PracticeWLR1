using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SecMan.BL;
using SecMan.BL.Common;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.IO.Abstractions;
using System.Net;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class PendingChangesManagerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly IPendingChangesManager _pendingChangesManager;
        private readonly Mock<IFileSystem> _mockFileSystem;

        public PendingChangesManagerTests()
        {
            _fixture = new Fixture();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockFileSystem = new Mock<IFileSystem>();
            _pendingChangesManager = new PendingChangesManager(_mockHttpContextAccessor.Object, _mockConfiguration.Object, _mockFileSystem.Object);
        }


        [Fact]
        public void GetChangedProperties_WhenAllPropertiesAreSame_ShouldReturnEmptyDictionary()
        {
            // Arrange
            UpdateUser? oldObject = _fixture.Create<UpdateUser>();
            UpdateUser? newObject = new UpdateUser();
            Log.Information("Completed Moqing dependencies");

            // Act
            Dictionary<string, object> result = _pendingChangesManager.GetChangedProperties(oldObject, newObject);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Empty(result);
            Log.Information("Verified that result is empty");
        }


        [Fact]
        public void GetChangedProperties_WhenNewObjectIsNull_ShouldReturnEmptyDictionary()
        {
            // Arrange
            UpdateUser? oldObject = _fixture.Create<UpdateUser>();
            UpdateUser? newObject = null;
            Log.Information("Completed Moqing dependencies");

            // Act
            Dictionary<string, object> result = _pendingChangesManager.GetChangedProperties(oldObject, newObject);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Empty(result);
            Log.Information("Verified that result is empty");
        }



        [Fact]
        public void GetChangedProperties_WhenPropertyChanges_ShouldReturnChangedProperty()
        {
            // Arrange
            UpdateUser oldObject = _fixture.Create<UpdateUser>();
            UpdateUser newObject = new UpdateUser();
            newObject.FirstName = "NewValue";
            Log.Information("Completed Moqing dependencies");

            // Act
            Dictionary<string, object> result = _pendingChangesManager.GetChangedProperties(oldObject, newObject);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Single(result);
            Log.Information("Verified that the result contains exactly one key-value pair.");
            Assert.Contains("firstName", result.Keys);
            Log.Information("Verified that the result contains the key 'firstName'.");
            Assert.Equal(newObject.FirstName, result["firstName"]);
            Log.Information("Verified that the value of 'firstName' in the result matches the expected value: {ExpectedFirstName}.", newObject.FirstName);
        }


        [Fact]
        public void GetChangedProperties_WhenPrimitivePropertyChanges_ShouldReturnChangedProperty()
        {
            // Arrange
            UpdateUser oldObject = _fixture.Create<UpdateUser>();
            UpdateUser newObject = new UpdateUser();
            newObject.IsActive = true;
            Log.Information("Completed Moqing dependencies");

            // Act
            Dictionary<string, object> result = _pendingChangesManager.GetChangedProperties(oldObject, newObject);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Single(result);
            Log.Information("Verified that the result contains exactly one key-value pair.");
            Assert.Contains("isActive", result.Keys);
            Log.Information("Verified that the result contains the key 'isActive'.");
            Assert.Equal(newObject.IsActive, result["isActive"]);
            Log.Information("Verified that the value of 'isActive' in the result matches the expected value: {ExpectedIsActive}.", newObject.IsActive);
        }



        [Fact]
        public void GetChangedProperties_WhenEnumerableChanges_ShouldReturnChangedEnumerable()
        {
            // Arrange
            UpdateUser oldObject = _fixture.Create<UpdateUser>();
            UpdateUser newObject = new UpdateUser();
            newObject.Roles = new List<ulong> { 5, 6, 7 };
            Log.Information("Completed Moqing dependencies");

            // Act
            Dictionary<string, object> result = _pendingChangesManager.GetChangedProperties(oldObject, newObject);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Single(result);
            Log.Information("Verified that the result contains exactly one key-value pair.");
            Assert.Contains("roles", result.Keys);
            Log.Information("Verified that the result contains the key 'roles'.");
            Assert.Equal(newObject.Roles, result["roles"]);
            Log.Information("Verified that the value of 'roles' in the result matches the expected value: {ExpectedRoles}.", newObject.Roles);
        }


        [Fact]
        public void GetChangedProperties_WhenNewValueIsNull_ShouldSkipProperty()
        {
            // Arrange
            UpdateUser oldObject = _fixture.Create<UpdateUser>();
            UpdateUser newObject = new UpdateUser();
            newObject.FirstName = null;
            Log.Information("Completed Moqing dependencies");

            // Act
            Dictionary<string, object> result = _pendingChangesManager.GetChangedProperties(oldObject, newObject);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.DoesNotContain("firstName", result.Keys);
            Log.Information("Verified that the result does not contains the key 'firstName'.");
        }


        [Fact]
        public async Task AddToSessionJsonAsync_ForUserEntity_WhenUserAlreadyExists_ShouldReturnConflict()
        {
            // Arrange
            var testData = new { username = "existing_user" };
            JsonEntities entity = JsonEntities.User;

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Create = new List<object>
                    {
                        new { username = "existing_user" }
                    }
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.AddToSessionJsonAsync(testData, entity);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.UserAlreadyExists);
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.Conflict);
        }


        [Fact]
        public async Task AddToSessionJsonAsync_ForUserEntity_WhenUserIsNew_ShouldReturnSuccess()
        {
            // Arrange
            var testData = new { username = "new_user" };
            JsonEntities entity = JsonEntities.User;

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Create = new List<object>
                    {
                        new { username = "existing_user" }
                    }
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.AddToSessionJsonAsync(testData, entity);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.OK);
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenChangesExistForUser_ShouldReturnSuccess()
        {
            // Arrange
            var testData = new { username = "updated_user" };
            var originalData = new { username = "original_user" };
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Update = new List<UpdateData>()
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.OK);
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenNoChangesExistForUser_ShouldReturnSuccessWithoutUpdate()
        {
            // Arrange
            var testData = new { username = "same_user" };
            var originalData = new { username = "same_user" };
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Update = new List<UpdateData>()
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.OK);
            _mockFileSystem.Verify(
                x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default),
                Times.Exactly(2)
            );
            Log.Information("Verified that the mock file system's WriteAllTextAsync method was called exactly twice.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenUserUpdateConflictsWithExistingUpdate_ShouldReturnConflict()
        {
            // Arrange
            var testData = new { username = "conflicting_user" };
            var originalData = new { username = "original_user" };
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Update = new List<UpdateData>
            {
                new UpdateData
                {
                    Id = 2,
                    Name = "other_user",
                    OldValue = new { username = "other_user" },
                    NewValue = new { username = "conflicting_user" }
                }
            },
                    Create = new List<object>()
                }
            };


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.UserAlreadyExists);
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.Conflict);
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenUserUpdateConflictsWithExistingCreate_ShouldReturnConflict()
        {
            // Arrange
            var testData = new { username = "conflicting_user" };
            var originalData = new { username = "original_user" };
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Update = new List<UpdateData>(),
                    Create = new List<object>
            {
                new { username = "conflicting_user" }
            }
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.UserAlreadyExists);
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.Conflict);
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenUserChangesExistAndNoConflicts_ShouldUpdateExistingData()
        {
            // Arrange
            var testData = new { username = "updated_user" };
            var originalData = new { username = "original_user" };
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Update = new List<UpdateData>
                    {
                        new UpdateData
                        {
                            Id = 1,
                            Name = "test_user",
                            OldValue = new { username = "original_user" },
                            NewValue = new { username = "old_update" }
                        }
                    },
                }
            };


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ExpectedMessage}'.", ResponseConstants.Success);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is '{ExpectedStatusCode}'.", HttpStatusCode.OK);
        }



        [Fact]
        public async Task DeleteToSessionJsonAsync_WhenUserDeleteIsUnique_ShouldAddToFile()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Delete = new List<DeleteData>(),
                    Update = new List<UpdateData>(),
                    Create = new List<object>()
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingUserData);


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.DeleteToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            UsersJsonData? resultData = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(resultData);
            Log.Information("Verified that the resultData object is not null.");
            Assert.Single(resultData.Users.Delete);
            Log.Information("Verified that there is exactly one entry in the 'Delete' list of the Users object.");
            Assert.Equal(id, resultData.Users.Delete[0].Id);
            Log.Information("Verified that the 'Id' of the first delete entry matches the expected value '{ExpectedId}'.", id);
            Assert.Equal(name, resultData.Users.Delete[0].Name);
            Log.Information("Verified that the 'Name' of the first delete entry matches the expected value '{ExpectedName}'.", name);
        }


        [Fact]
        public async Task DeleteToSessionJsonAsync_WhenUserDeleteAlreadyExists_ShouldNotDuplicate()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 1;
            string name = "test_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Delete = new List<DeleteData>
            {
                new DeleteData { Id = id, Name = name }
            },
                    Update = new List<UpdateData>(),
                    Create = new List<object>()
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingUserData);

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.DeleteToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            UsersJsonData? resultData = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(resultData);
            Log.Information("Verified that the resultData object is not null.");
            Assert.Single(resultData.Users.Delete);
            Log.Information("Verified that there is exactly one entry in the 'Delete' list of the Users object.");
        }


        [Fact]
        public async Task DeleteToSessionJsonAsync_WhenMultipleUserDeletes_ShouldAddOnlyNewOnes()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 2;
            string name = "new_user";

            UsersJsonData existingUserData = new UsersJsonData
            {
                Users = new UserUnsavedChanges
                {
                    Delete = new List<DeleteData>
            {
                new DeleteData { Id = 1, Name = "existing_user" }
            },
                    Update = new List<UpdateData>(),
                    Create = new List<object>()
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingUserData);

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.DeleteToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            UsersJsonData? resultData = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(resultData);
            Log.Information("Verified that the resultData object is not null.");
            Assert.Equal(2, resultData.Users.Delete.Count);
            Log.Information("Verified that there are exactly two entries in the 'Delete' list of the Users object.");
            Assert.Contains(resultData.Users.Delete, x => x.Id == id && x.Name == name);
            Log.Information("Verified that the 'Delete' list contains an entry with the specified Id and Name.");
        }


        [Fact]
        public async Task RetireToSessionJsonAsync_Should_Add_RetireData_When_Valid()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 123;
            string name = "Test User";

            UserUnsavedChanges mockRetireData = new UserUnsavedChanges();
            string existingJson = JsonConvert.SerializeObject(mockRetireData);
            string updatedJson = string.Empty;


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.RetireToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            _mockFileSystem.Verify(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
            UsersJsonData? result = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Single(result.Users.Retire);
            Log.Information("Verified that there is exactly one entry in the 'Retire' list of the Users object.");
            Assert.Contains(result.Users.Retire, r => r.Id == id && r.Name == name);
            Log.Information("Verified that the 'Retire' list contains an entry with the specified Id and Name.");

        }

        [Fact]
        public async Task RetireToSessionJsonAsync_Should_Not_Add_Duplicate_RetireData()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 123;
            string name = "Test User";

            UserUnsavedChanges mockRetireData = new UserUnsavedChanges
            {
                Retire = new List<RetireData>
        {
            new RetireData { Id = id, Name = name }
        }
            };

            string existingJson = JsonConvert.SerializeObject(mockRetireData);
            string updatedJson = string.Empty;

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.RetireToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            _mockFileSystem.Verify(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
            UsersJsonData? result = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Single(result.Users.Retire);
            Log.Information("Verified that there is exactly one entry in the 'Retire' list of the Users object. Ensuring no duplicates.");
            Assert.Contains(result.Users.Retire, r => r.Id == id && r.Name == name);
            Log.Information("Verified that the 'Retire' list contains an entry with the specified Id and Name.");
        }

        [Fact]
        public async Task RetireToSessionJsonAsync_Should_Handle_Empty_JsonFile()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 456;
            string name = "New User";

            string existingJson = "{}"; // Empty JSON
            string updatedJson = string.Empty;

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.RetireToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            _mockFileSystem.Verify(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
            UsersJsonData? result = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Single(result.Users.Retire);
            Log.Information("Verified that there is exactly one entry in the 'Retire' list of the Users object.");
            Assert.Contains(result.Users.Retire, r => r.Id == id && r.Name == name);
            Log.Information("Verified that the 'Retire' list contains an entry with the specified Id and Name.");
        }



        [Fact]
        public async Task UnlockToSessionJsonAsync_Should_Add_RetireData_When_Valid()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 123;
            string name = "Test User";
            bool changePasswordOnLogin = true;

            UserUnsavedChanges mockUnlockData = new UserUnsavedChanges();
            string existingJson = JsonConvert.SerializeObject(mockUnlockData);
            string updatedJson = string.Empty;


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.UnlockToSessionJsonAsync(entity, id, name, changePasswordOnLogin);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            _mockFileSystem.Verify(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
            UsersJsonData? result = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Single(result.Users.Unlock);
            Log.Information("Verified that there is exactly one entry in the 'Unlock' list of the Users object.");
            Assert.Contains(result.Users.Unlock, r => r.Id == id && r.Name == name);
            Log.Information("Verified that the 'Unlock' list contains an entry with the specified Id and Name.");
        }

        [Fact]
        public async Task UnlockToSessionJsonAsync_Should_Not_Add_Duplicate_RetireData()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 123;
            string name = "Test User";
            bool changePasswordOnLogin = true;

            UserUnsavedChanges mockUnlockData = new UserUnsavedChanges
            {
                Unlock = new List<UnlockData>
                {
                    new UnlockData { Id = id, Name = name ,ChangePasswordOnLogin = true}
                }
            };

            string existingJson = JsonConvert.SerializeObject(mockUnlockData);
            string updatedJson = string.Empty;

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.UnlockToSessionJsonAsync(entity, id, name, changePasswordOnLogin);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            _mockFileSystem.Verify(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
            UsersJsonData? result = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Single(result.Users.Unlock);
            Log.Information("Verified that there is exactly one entry in the 'Unlock' list of the Users object.");
            Assert.Contains(result.Users.Unlock, r => r.Id == id && r.Name == name);
            Log.Information("Verified that the 'Unlock' list contains an entry with the specified Id and Name.");
        }

        [Fact]
        public async Task UnlockToSessionJsonAsync_Should_Handle_Empty_JsonFile()
        {
            // Arrange
            JsonEntities entity = JsonEntities.User;
            ulong id = 456;
            string name = "New User";
            bool changePasswordOnLogin = true;

            string existingJson = "{}"; // Empty JSON
            string updatedJson = string.Empty;

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.UnlockToSessionJsonAsync(entity, id, name, changePasswordOnLogin);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            _mockFileSystem.Verify(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
            UsersJsonData? result = JsonConvert.DeserializeObject<UsersJsonData>(updatedJson);
            Log.Information("Deserialized the updated JSON data into a UsersJsonData object.");
            Assert.NotNull(result);
            Log.Information("Verified that the result object is not null.");
            Assert.Single(result.Users.Unlock);
            Log.Information("Verified that there is exactly one entry in the 'Unlock' list of the Users object.");
            Assert.Contains(result.Users.Unlock, r => r.Id == id && r.Name == name);
            Log.Information("Verified that the 'Unlock' list contains an entry with the specified Id and Name.");
        }



        [Fact]
        public async Task AddToSessionJsonAsync_ForRoleEntity_WhenUserAlreadyExists_ShouldReturnConflict()
        {
            // Arrange
            var testData = new { name = "existing_role" };
            JsonEntities entity = JsonEntities.Role;

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Create = new List<object>
                    {
                        new { name = "existing_role" }
                    }
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingRoleData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.AddToSessionJsonAsync(testData, entity);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(RoleResponseConstants.RoleAlreadyExists, result.Message);
            Log.Information("Verified that the result message is '{RoleAlreadyExists}'.");
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.Conflict}.");
        }


        [Fact]
        public async Task AddToSessionJsonAsync_ForRoleEntity_WhenUserIsNew_ShouldReturnSuccess()
        {
            // Arrange
            var testData = new { name = "new_role" };
            JsonEntities entity = JsonEntities.Role;

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Create = new List<object>
                    {
                        new { name = "existing_role" }
                    }
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingRoleData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.AddToSessionJsonAsync(testData, entity);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ResponseConstants.Success}'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.OK}.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenChangesExistForRole_ShouldReturnSuccess()
        {
            // Arrange
            var testData = new { name = "updated_role" };
            var originalData = new { name = "original_role" };
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Update = new List<UpdateData>()
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingRoleData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ResponseConstants.Success}'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.OK}.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenNoChangesExistForRole_ShouldReturnSuccessWithoutUpdate()
        {
            // Arrange
            var testData = new { name = "same_role" };
            var originalData = new { name = "same_role" };
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Update = new List<UpdateData>()
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingRoleData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ResponseConstants.Success}'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.OK}.");
            _mockFileSystem.Verify(
                x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default),
                Times.Exactly(2)
            );
            Log.Information("Verified that WriteAllTextAsync was called exactly 2 times.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenRoleUpdateConflictsWithExistingUpdate_ShouldReturnConflict()
        {
            // Arrange
            var testData = new { name = "conflicting_role" };
            var originalData = new { name = "original_role" };
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Update = new List<UpdateData>
                    {
                        new UpdateData
                        {
                            Id = 2,
                            Name = "other_role",
                            OldValue = new { name = "other_role" },
                            NewValue = new { name = "conflicting_role" }
                        }
                    },
                    Create = new List<object>()
                }
            };


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingRoleData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(RoleResponseConstants.RoleAlreadyExists, result.Message);
            Log.Information("Verified that the result message is '{RoleResponseConstants.RoleAlreadyExists}'.");
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.Conflict}.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenRoleUpdateConflictsWithExistingCreate_ShouldReturnConflict()
        {
            // Arrange
            var testData = new { name = "conflicting_role" };
            var originalData = new { name = "original_role" };
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Update = new List<UpdateData>(),
                    Create = new List<object>
                    {
                        new { name = "conflicting_role" }
                    }
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingRoleData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(RoleResponseConstants.RoleAlreadyExists, result.Message);
            Log.Information("Verified that the result message is '{RoleResponseConstants.RoleAlreadyExists}'.");
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.Conflict}.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenRoleChangesExistAndNoConflicts_ShouldUpdateExistingData()
        {
            // Arrange
            var testData = new { name = "updated_role" };
            var originalData = new { name = "original_role" };
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingUserData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Update = new List<UpdateData>
                    {
                        new UpdateData
                        {
                            Id = 1,
                            Name = "test_role",
                            OldValue = new { name = "original_role" },
                            NewValue = new { name = "old_update" }
                        }
                    },
                }
            };


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingUserData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message is '{ResponseConstants.Success}'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is {HttpStatusCode.OK}.");
        }



        [Fact]
        public async Task DeleteToSessionJsonAsync_WhenRoleDeleteIsUnique_ShouldAddToFile()
        {
            // Arrange
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Delete = new List<DeleteData>(),
                    Update = new List<UpdateData>(),
                    Create = new List<object>()
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingRoleData);


            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.DeleteToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            RolesJsonData? resultData = JsonConvert.DeserializeObject<RolesJsonData>(updatedJson);
            Assert.NotNull(resultData);
            Log.Information("Verified that the deserialized result data is not null.");
            Assert.Single(resultData.Roles.Delete);
            Log.Information("Verified that there is exactly one role in the 'Delete' list.");
            Assert.Equal(id, resultData.Roles.Delete[0].Id);
            Log.Information("Verified that the role ID in the 'Delete' list matches the expected ID.");
            Assert.Equal(name, resultData.Roles.Delete[0].Name);
            Log.Information("Verified that the role Name in the 'Delete' list matches the expected Name.");
        }


        [Fact]
        public async Task DeleteToSessionJsonAsync_WhenRoleDeleteAlreadyExists_ShouldNotDuplicate()
        {
            // Arrange
            JsonEntities entity = JsonEntities.Role;
            ulong id = 1;
            string name = "test_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Delete = new List<DeleteData>
                    {
                        new DeleteData { Id = id, Name = name }
                    },
                    Update = new List<UpdateData>(),
                    Create = new List<object>()
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingRoleData);

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.DeleteToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            RolesJsonData? resultData = JsonConvert.DeserializeObject<RolesJsonData>(updatedJson);
            Assert.NotNull(resultData);
            Log.Information("Verified that the deserialized result data is not null.");
            Assert.Single(resultData.Roles.Delete);
            Log.Information("Verified that there is exactly one role in the 'Delete' list.");

        }


        [Fact]
        public async Task DeleteToSessionJsonAsync_WhenMultipleRoleDeletes_ShouldAddOnlyNewOnes()
        {
            // Arrange
            JsonEntities entity = JsonEntities.Role;
            ulong id = 2;
            string name = "new_role";

            RolesJsonData existingRoleData = new RolesJsonData
            {
                Roles = new UnsavedChanges
                {
                    Delete = new List<DeleteData>
                    {
                        new DeleteData { Id = 1, Name = "existing_role" }
                    },
                    Update = new List<UpdateData>(),
                    Create = new List<object>()
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingRoleData);

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            await _pendingChangesManager.DeleteToSessionJsonAsync(entity, id, name);
            Log.Information("Test result: {@Result}", updatedJson);

            // Assert
            RolesJsonData? resultData = JsonConvert.DeserializeObject<RolesJsonData>(updatedJson);
            Assert.NotNull(resultData);
            Log.Information("Verified that the deserialized result data is not null.");
            Assert.Equal(2, resultData.Roles.Delete.Count);
            Log.Information("Verified that there are exactly 2 roles in the 'Delete' list.");
            Assert.Contains(resultData.Roles.Delete, x => x.Id == id && x.Name == name);
            Log.Information("Verified that the 'Delete' list contains a role with the expected Id and Name.");
        }



        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenChangesExistForSystemFeatures_ShouldReturnSuccess()
        {
            // Arrange
            var testData = new { name = "updated_role", id = 1, value = "test" };
            var originalData = new { name = "updated_role", id = 1, value = "updated_value" };
            JsonEntities entity = JsonEntities.SystemFeature;
            ulong id = 1;
            string name = "test_system_feature";

            SystemFeatureJsonData existingSystemFeatureData = new SystemFeatureJsonData
            {
                Settings = new SystemFeaturesUnSavedChanges
                {
                    Update = new List<SystemFeaturesUpdateData>()
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingSystemFeatureData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message equals 'Success'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is 'OK' (200).");
        }




        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenNoChangesExistForSystemFeature_ShouldReturnSuccessWithoutUpdate()
        {
            // Arrange
            var testData = new { name = "updated_role", id = 1, value = "test" };
            var originalData = new { name = "updated_role", id = 1, value = "test" };
            JsonEntities entity = JsonEntities.SystemFeature;
            ulong id = 1;
            string name = "test_system_feature";

            SystemFeatureJsonData existingSystemFeatureData = new SystemFeatureJsonData
            {
                Settings = new SystemFeaturesUnSavedChanges
                {
                    Update = new List<SystemFeaturesUpdateData>()
                }
            };

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(JsonConvert.SerializeObject(existingSystemFeatureData));

            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message equals 'Success'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is 'OK' (200).");
            _mockFileSystem.Verify(
                x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default),
                Times.Exactly(2)
            );
            Log.Information("Verified that the WriteAllTextAsync method was called exactly 2 times.");
        }


        [Fact]
        public async Task UpdateToSessionJsonAsync_WhenChangesExistForSystemFeature_ShouldReturnSuccessWithUpdate()
        {
            // Arrange
            UpdateSystemPolicyData testData = new UpdateSystemPolicyData { Name = "policy", Id = 1, Value = "updated_policy" };
            UpdateSystemPolicyData originalData = new UpdateSystemPolicyData { Name = "policy", Id = 1, Value = "original_policy" };
            JsonEntities entity = JsonEntities.SystemFeature;
            ulong id = 1;
            string name = "system_feature";

            SystemFeatureJsonData existingSystemFeatureData = new SystemFeatureJsonData
            {
                Settings = new SystemFeaturesUnSavedChanges
                {
                    Update = new List<SystemFeaturesUpdateData>()
                    {
                        new SystemFeaturesUpdateData
                        {
                            Id = id,
                            Name = name,
                            OldValue = [originalData],
                            NewValue = [new UpdateSystemPolicyData { Name = "policy", Id = 1, Value = "old_policy" }],
                        }
                    }
                }
            };

            string existingJson = JsonConvert.SerializeObject(existingSystemFeatureData);

            _mockConfiguration.Setup(c => c.GetSection("SessionFilesPath").Value).Returns("C:/path/to/");

            _mockFileSystem.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Returns(false);

            _mockFileSystem
                .Setup(x => x.File.ReadAllTextAsync(It.IsAny<string>(), default))
                .ReturnsAsync(existingJson);

            string updatedJson = string.Empty;
            _mockFileSystem
                .Setup(x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((_, json, _) => updatedJson = json)
                .Returns(Task.CompletedTask);
            Log.Information("Completed Moqing dependencies");

            // Act
            ApiResponse result = await _pendingChangesManager.UpdateToSessionJsonAsync(testData, entity, originalData, id, name);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result);
            Log.Information("Verified that the result is not null.");
            Assert.Equal(ResponseConstants.Success, result.Message);
            Log.Information("Verified that the result message equals 'Success'.");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Log.Information("Verified that the result status code is 'OK' (200).");
            _mockFileSystem.Verify(
                x => x.File.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), default),
                Times.Exactly(2)
            );
            Log.Information("Verified that the WriteAllTextAsync method was called exactly 2 times.");
        }
    }
}
