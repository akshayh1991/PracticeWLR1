using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using SecMan.BL;
using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;

namespace SecMan.UnitTests.UserAccessManagement
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class SystemFeatureTests
    {
        private readonly DbContextOptions<Db> _dbContextOptions;
        private readonly Db _db;
        private readonly IFixture _fixture;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly Mock<IPendingChangesManager> _mockPendingChangesManager;
        private SystemFeatureBL _systemFeatureBL;
        private readonly Mock<IHttpContextAccessor> _mockHttpContext;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public SystemFeatureTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                    .OfType<ThrowingRecursionBehavior>()
                    .ToList()
                    .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockPendingChangesManager = new Mock<IPendingChangesManager>();
            _dbContextOptions = new DbContextOptionsBuilder<Db>()
            .UseSqlite("DataSource=:memory:")
            .Options;
            string[] allowedTypes = new[] { "Email", "Bool", "Int" };
            _db = new Db(_dbContextOptions, string.Empty);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();
            _db.SysFeats.AddRange(
                _fixture.Build<SysFeat>()
                .With(x => x.Id, 0ul)
                .With(x => x.Common, true)
                .With(x => x.SysFeatProps, allowedTypes.SelectMany(type =>
                                            _fixture.Build<SysFeatProp>()
                                            .With(x => x.Id, 0ul)
                                            .With(x => x.ValType, type)
                                            .With(x => x.ValMin, 0ul)
                                            .With(x => x.ValMax, 1000ul)
                                            .With(x => x.Langs,
                                                    (
                                                        _fixture.Build<SysFeatPropLang>()
                                                        .With(x => x.Id, 0ul)
                                                        .With(x => x.Code, "en")
                                                        .CreateMany(5).ToList()
                                                    ))
                                            .CreateMany(1))
                .ToList())
                .CreateMany(3).ToList()
                );
            _db.SysFeats.AddRange(
                _fixture.Build<SysFeat>()
                .With(x => x.Id, 0ul)
                .With(x => x.Common, false)
                .With(x => x.SysFeatProps, (
                                            _fixture.Build<SysFeatProp>()
                                            .With(x => x.Id, 0ul)
                                            .With(x => x.ValMin, 0ul)
                                            .With(x => x.ValMax, 1000ul)
                                            .With(x => x.ValType, () => allowedTypes[_fixture.Create<int>() % allowedTypes.Length])
                                            .With(x => x.Langs,
                                                    (
                                                        _fixture.Build<SysFeatPropLang>()
                                                        .With(x => x.Id, 0ul)
                                                        .With(x => x.Code, "fr")
                                                        .CreateMany(5).ToList()
                                                    ))
                                            .CreateMany(5).ToList()
                                           ))
                .CreateMany(3).ToList()
                );
            _db.SaveChanges();

            _mockHttpContext = new Mock<IHttpContextAccessor>();
            _mockConfiguration = new Mock<IConfiguration>();

            _unitOfWork = new UnitOfWork(_db, _mockHttpContext.Object, _mockConfiguration.Object);

            _systemFeatureBL = new SystemFeatureBL(_unitOfWork, _mockPendingChangesManager.Object, _mockCurrentUserService.Object);

        }


        // GetSystemPoliciesAsync
        [Fact]
        public async Task GetSystemPoliciesAsync_ShouldReturnOnlyCommonPolicies_WhenIsCommonTrue()
        {
            // Arrange
            bool isCommon = true;

            // Act
            ServiceResponse<List<SystemPolicies>>? result = await _systemFeatureBL.GetSystemPoliciesAsync(isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(3, result?.Data?.Count);
            Log.Information("Verified if the returned array length matchs the seeded data count");
            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.True(result?.Data?.TrueForAll(x => x.IsCommon));
            Log.Information("Verified if the all the returned objects are common policies");
        }


        [Fact]
        public async Task GetSystemPoliciesAsync_ShouldReturnOnlyNonCommonPolicies_WhenIsCommonFalse()
        {
            // Arrange
            bool isCommon = false;

            // Act
            ServiceResponse<List<SystemPolicies>>? result = await _systemFeatureBL.GetSystemPoliciesAsync(isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Equal(6, result?.Data?.Count);
            Log.Information("Verified if the returned array length matchs the seeded data count");
            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.False(result?.Data?.TrueForAll(x => !x.IsCommon));
            Log.Information("Verified if the all the returned objects are both common and non common policies");
        }

        // GetSystemPolicyByIdAsync
        [Fact]
        public async Task GetSystemPolicyByIdAsync_ShouldReturn_InvalidId_WhenSystemFeatureWithIdNotExists_AndIsCommonTrue()
        {
            // Arrange
            ulong sysFeatureId = 7ul;
            bool isCommon = true;

            // Act
            ServiceResponse<List<SystemPolicyData>>? result = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result?.Data);
            Log.Information("Verified that Response data is null");
            Assert.Equal(HttpStatusCode.NotFound, result?.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.SettingDoesNotExists, result?.Message);
            Log.Information("Verified if the result object's message is InvalidId");
        }

        [Fact]
        public async Task GetSystemPolicyByIdAsync_ShouldReturn_InvalidId_WhenSystemFeatureWithIdNotExists_AndIsCommonFalse()
        {
            // Arrange
            ulong sysFeatureId = 7ul;
            bool isCommon = false;

            // Act
            ServiceResponse<List<SystemPolicyData>>? result = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result?.Data);
            Log.Information("Verified that Response data is null");
            Assert.Equal(HttpStatusCode.NotFound, result?.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.SecurityPolicyDoesNotExists, result?.Message);
            Log.Information("Verified if the result object's message is InvalidId");
        }


        [Fact]
        public async Task GetSystemPolicyByIdAsync_ShouldReturn_InvalidId_WhenSystemFeatureWithIdExistsButNotCommon_WhenPassing_IsCommonTrue()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = true;

            // Act
            ServiceResponse<List<SystemPolicyData>>? result = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result?.Data);

            Assert.Equal(HttpStatusCode.NotFound, result?.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.SettingDoesNotExists, result?.Message);
            Log.Information("Verified if the result object's message is InvalidId");
        }


        [Fact]
        public async Task GetSystemPolicyByIdAsync_ShouldReturn_PoliciesList_WhenSystemFeatureWithIdExists_And_Common_WhenPassing_IsCommonTrue()
        {
            // Arrange
            ulong sysFeatureId = 1ul;
            bool isCommon = true;

            // Act
            ServiceResponse<List<SystemPolicyData>>? result = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result?.Data);

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result?.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        [Fact]
        public async Task GetSystemPolicyByIdAsync_ShouldReturn_PoliciesList_SpecificToLanguage()
        {
            // Arrange
            ulong sysFeatureId = 1ul;
            bool isCommon = true;
            UserDetails mockUserDetails = _fixture.Build<UserDetails>()
                .With(x => x.Language, "en")
                .Create();
            _mockCurrentUserService.Setup(x => x.UserDetails)
                .Returns(mockUserDetails);

            // Act
            ServiceResponse<List<SystemPolicyData>>? result = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result?.Data);
            Log.Information("Verified that Response data is not null");
            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result?.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        [Fact]
        public async Task GetSystemPolicyByIdAsync_ShouldReturn_PoliciesList_SpecificToLanguage_When_IsCommonFalse()
        {
            // Arrange
            ulong sysFeatureId = 1ul;
            bool isCommon = false;
            UserDetails mockUserDetails = _fixture.Build<UserDetails>()
                .With(x => x.Language, "en")
                .Create();
            _mockCurrentUserService.Setup(x => x.UserDetails)
                .Returns(mockUserDetails);

            // Act
            ServiceResponse<List<SystemPolicyData>>? result = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result?.Data);

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result?.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        // UpdateSystemPolicyByIdAsync
        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_InvalidId_WhenSystemFeatureWithIdNotExists()
        {
            // Arrange
            ulong sysFeatureId = 7ul;
            bool isCommon = true;
            List<UpdateSystemPolicyData> mockInput = _fixture.Build<UpdateSystemPolicyData>()
                .CreateMany(5)
                .ToList();

            // Act
            ServiceResponse<List<UpdatedResponse>>? result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, mockInput, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result?.Data);
            Log.Information("Verified that Response data is not null");
            Assert.Equal(HttpStatusCode.NotFound, result?.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.SettingDoesNotExists, result?.Message);
            Log.Information("Verified if the result object's message is InvalidId");
        }


        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_InvalidId_WhenSystemFeatureWithIdExistsButNotCommon_WhenPassing_IsCommonTrue()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = true;
            List<UpdateSystemPolicyData> mockInput = _fixture.Build<UpdateSystemPolicyData>()
                            .CreateMany(5)
                            .ToList();

            // Act
            ServiceResponse<List<UpdatedResponse>>? result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, mockInput, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.Null(result?.Data);
            Log.Information("Verified that Response data is null");
            Assert.Equal(HttpStatusCode.NotFound, result?.StatusCode);
            Log.Information("Verified if the result object's status code is NotFound:404");
            Assert.Equal(ResponseConstants.SettingDoesNotExists, result?.Message);
            Log.Information("Verified if the result object's message is InvalidId");
        }


        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_IfEmailIsNotValid()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = false;
            ServiceResponse<List<SystemPolicyData>>? policies = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            policies?.Data?.ForEach(x =>
            {
                if (x.Type == nameof(SystemPolicyTypes.Email))
                {
                    x.Value = "testEmail";
                }
                if (x.Type == nameof(SystemPolicyTypes.Bool))
                {
                    x.Value = "true";
                }
                if (x.Type == nameof(SystemPolicyTypes.Int))
                {
                    x.Value = "100";
                }
            });
            List<UpdateSystemPolicyData>? input = policies?.Data.Select(x => new UpdateSystemPolicyData
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,
            }).ToList();
            ModelStateDictionary modelState = new ModelStateDictionary();

            // Act
            ServiceResponse<List<UpdatedResponse>> result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, input, isCommon, modelState: modelState);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotEmpty(modelState);
            Log.Information("Verified If Model State Is Not Empty");
            Assert.False(modelState.IsValid);
            Log.Information("Verified If ModelState Is Failing");
            Assert.Equal(ValidationConstants.InvalidEmail, modelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            Log.Information("Verified if the model state error message is InvalidEmail");
        }

        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_IfBooleanIsNotValid()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = false;
            ServiceResponse<List<SystemPolicyData>>? policies = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            policies?.Data?.ForEach(x =>
            {
                if (x.Type == nameof(SystemPolicyTypes.Email))
                {
                    x.Value = "";
                }
                if (x.Type == nameof(SystemPolicyTypes.Bool))
                {
                    x.Value = "xyz";
                }
                if (x.Type == nameof(SystemPolicyTypes.Int))
                {
                    x.Value = "100";
                }
            });
            List<UpdateSystemPolicyData>? input = policies?.Data.Select(x => new UpdateSystemPolicyData
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,
            }).ToList();
            ModelStateDictionary modelState = new ModelStateDictionary();

            // Act
            ServiceResponse<List<UpdatedResponse>> result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, input, isCommon, modelState: modelState);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotEmpty(modelState);
            Log.Information("Verified If Model State Is Not Empty");
            Assert.False(modelState.IsValid);
            Log.Information("Verified If ModelState Is Failing");
            Assert.Equal(ValidationConstants.InvalidBoolean, modelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            Log.Information("Verified if the model state error message is InvalidBoolean");
        }


        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_IfIntIsNotValid()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = false;
            ServiceResponse<List<SystemPolicyData>>? policies = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            policies?.Data?.ForEach(x =>
            {
                if (x.Type == nameof(SystemPolicyTypes.Email))
                {
                    x.Value = "";
                }
                if (x.Type == nameof(SystemPolicyTypes.Bool))
                {
                    x.Value = "true";
                }
                if (x.Type == nameof(SystemPolicyTypes.Int))
                {
                    x.Value = "100abc";
                }
            });
            List<UpdateSystemPolicyData>? input = policies?.Data.Select(x => new UpdateSystemPolicyData
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,
            }).ToList();
            ModelStateDictionary modelState = new ModelStateDictionary();

            // Act
            ServiceResponse<List<UpdatedResponse>> result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, input, isCommon, modelState: modelState);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotEmpty(modelState);
            Log.Information("Verified If Model State Is Not Empty");
            Assert.False(modelState.IsValid);
            Log.Information("Verified If ModelState Is Failing");
            Assert.Equal(ValidationConstants.InvalidNumber, modelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
            Log.Information("Verified if the model state error message is InvalidNumber");
        }


        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_OkWith_UpdatedObjects()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = false;
            ServiceResponse<List<SystemPolicyData>>? policies = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            policies?.Data?.ForEach(x =>
            {
                if (x.Type == nameof(SystemPolicyTypes.Email))
                {
                    x.Value = "";
                }
                if (x.Type == nameof(SystemPolicyTypes.Bool))
                {
                    x.Value = "true";
                }
                if (x.Type == nameof(SystemPolicyTypes.Int))
                {
                    x.Value = "100";
                }
            });
            List<UpdateSystemPolicyData>? input = policies?.Data.Select(x => new UpdateSystemPolicyData
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,
            }).ToList();

            // Act
            ServiceResponse<List<UpdatedResponse>>? result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, input, isCommon);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result?.Data);
            Log.Information("Verified that Response data is not null");
            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result?.Message);
            Log.Information("Verified if the result object's message is Success");
        }


        [Fact]
        public async Task UpdateSystemPolicyByIdAsync_ShouldReturn_OkWith_UpdatedObjects_WhenSaveToDbIsTrue()
        {
            // Arrange
            ulong sysFeatureId = 6ul;
            bool isCommon = false;
            ServiceResponse<List<SystemPolicyData>>? policies = await _systemFeatureBL.GetSystemPolicyByIdAsync(sysFeatureId, isCommon);
            policies?.Data?.ForEach(x =>
            {
                if (x.Type == nameof(SystemPolicyTypes.Email))
                {
                    x.Value = "";
                }
                if (x.Type == nameof(SystemPolicyTypes.Bool))
                {
                    x.Value = "true";
                }
                if (x.Type == nameof(SystemPolicyTypes.Int))
                {
                    x.Value = "100";
                }
            });
            List<UpdateSystemPolicyData>? input = policies?.Data.Select(x => new UpdateSystemPolicyData
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,
            }).ToList();

            // Act
            ServiceResponse<List<UpdatedResponse>>? result = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(sysFeatureId, input, isCommon, true);
            Log.Information("Test result: {@Result}", result);

            // Assert
            Assert.NotNull(result?.Data);
            Log.Information("Verified that Response data is not null");
            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Log.Information("Verified if the result object's status code is Ok:200");
            Assert.Equal(ResponseConstants.Success, result?.Message);
            Log.Information("Verified if the result object's message is Success");
        }
    }
}
