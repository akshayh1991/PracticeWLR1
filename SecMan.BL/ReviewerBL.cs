using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.IO.Abstractions;
using System.Net;

namespace SecMan.BL
{
    public class ReviewerBl : IReviewerBl
    {
        private readonly IUserBL _userBL;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ISystemFeatureBL _systemFeatureBL;
        private readonly IRoleBL _roleBL;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IFileSystem _fileSystem;
        private readonly IDeviceBL _deviceBL;

        public ReviewerBl(IUserBL userBL,
                          IUnitOfWork unitOfWork,
                          IConfiguration configuration,
                          ISystemFeatureBL systemFeatureBL,
                          IRoleBL roleBL,
                          IHttpContextAccessor httpContext,
                          IFileSystem fileSystem,
                          IDeviceBL deviceBL)
        {
            _userBL = userBL;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _systemFeatureBL = systemFeatureBL;
            _roleBL = roleBL;
            _httpContext = httpContext;
            _fileSystem = fileSystem;
            _deviceBL = deviceBL;
        }

        public async Task<ServiceResponse<JObject>> ReadJsonData()
        {
            string jsonFileName = $"{_configuration.GetSection("SessionFilesPath").Value}{_httpContext.HttpContext?.User?.FindFirst(ResponseHeaders.SSOSessionId).Value}.json";

            string jsonString = string.Empty;
            if (_fileSystem.File.Exists(jsonFileName))
            {
                jsonString = await _fileSystem.File.ReadAllTextAsync(jsonFileName);
            }

            JObject? jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return new(ResponseConstants.Success, HttpStatusCode.OK, jsonObject);
        }


        private bool HasChanges(IEnumerable<object> collection)
        {
            return collection.Any();
        }


        public async Task<ApiResponse> SaveUnsavedJsonChanges(JObject model)
        {
            string jsonFileName = $"{_configuration.GetSection("SessionFilesPath").Value}{_httpContext.HttpContext?.User?.FindFirst(ResponseHeaders.SSOSessionId).Value}.json";
            if (!_fileSystem.Directory.Exists(_configuration.GetSection("SessionFilesPath").Value))
            {
                _fileSystem.Directory.CreateDirectory(_configuration.GetSection("SessionFilesPath").Value);
            }


            if (!_fileSystem.File.Exists(jsonFileName))
            {
                await _fileSystem.File.WriteAllTextAsync(jsonFileName, "{}");
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            string jsonText = JsonConvert.SerializeObject(model, settings);

            await _fileSystem.File.WriteAllTextAsync(jsonFileName, jsonText);
            return new(ResponseConstants.Success, HttpStatusCode.OK);
        }


        public async Task<ApiResponse> SaveUnsavedChanges(JObject model)
        {
            await _unitOfWork.BeginTransactionAsync();
            List<ApiResponse> errors = new List<ApiResponse>();
            UsersJsonData? userData = model.ToObject<UsersJsonData>();
            SystemFeatureJsonData? systemFeatureData = model.ToObject<SystemFeatureJsonData>();
            RolesJsonData? roleData = model.ToObject<RolesJsonData>();
            DeviceJsonData? deviceData = model.ToObject<DeviceJsonData>();

            bool userDataHasChanges = HasChanges(userData.Users.Create) || HasChanges(userData.Users.Update) || HasChanges(userData.Users.Delete) || HasChanges(userData.Users.Retire) || HasChanges(userData.Users.Unlock);
            bool systemFeatureDataHasChanges = HasChanges(systemFeatureData.Settings.Update);
            bool roleDataHasChanges = HasChanges(roleData.Roles.Update) || HasChanges(roleData.Roles.Create) || HasChanges(roleData.Roles.Delete);
            bool deviceDataHasChanges = HasChanges(deviceData.Devices.Update) || HasChanges(deviceData.Devices.Create) || HasChanges(deviceData.Devices.Delete);

            if ((userDataHasChanges && systemFeatureDataHasChanges) || 
                (roleDataHasChanges && userDataHasChanges) || 
                (roleDataHasChanges && systemFeatureDataHasChanges)||
                (userDataHasChanges && deviceDataHasChanges) ||
                (roleDataHasChanges && deviceDataHasChanges) ||
                (systemFeatureDataHasChanges && deviceDataHasChanges)) 
            {
                throw new InvalidOperationException(ResponseConstants.MultipleChangeAreNotAllowed);
            }

            if (userDataHasChanges)
            {
                List<ApiResponse> res = await SaveUserData(userData.Users);
                errors.AddRange(res);
            }
            if (systemFeatureDataHasChanges)
            {
                List<ApiResponse> res = await SaveSystemFeatureData(systemFeatureData.Settings);
                errors.AddRange(res);
            }
            if (roleDataHasChanges)
            {
                List<ApiResponse> res = await SaveRoleData(roleData.Roles);
                errors.AddRange(res);
            }
            if (deviceDataHasChanges)
            {
                List<ApiResponse> res = await SaveDeviceData(deviceData.Devices);
                errors.AddRange(res);
            }

            if (errors.Exists(x => x.StatusCode != HttpStatusCode.OK && x.StatusCode != HttpStatusCode.Created))
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new(ResponseConstants.NotFullyProcessed, HttpStatusCode.InternalServerError);
            }
            await _unitOfWork.CommitTransactionAsync();

            string jsonFileName = $"{_configuration.GetSection("SessionFilesPath").Value}{_httpContext.HttpContext?.User?.FindFirst(ResponseHeaders.SSOSessionId).Value}.json";
            if (_fileSystem.File.Exists(jsonFileName))
            {
                _fileSystem.File.Delete(jsonFileName);
            }
            return new(ResponseConstants.Success, HttpStatusCode.OK);
        }


        private async Task<List<ApiResponse>> SaveSystemFeatureData(SystemFeaturesUnSavedChanges systemFeaturesUnSavedChanges)
        {
            List<ApiResponse> errors = new List<ApiResponse>();

            if (systemFeaturesUnSavedChanges != null && systemFeaturesUnSavedChanges.Update.Any())
            {
                foreach (SystemFeaturesUpdateData systemFeature in systemFeaturesUnSavedChanges.Update)
                {
                    for (int index = 0; index < systemFeature.OldValue.Count; index++)
                    {
                        if (index < systemFeature.NewValue.Count)
                        {
                            JObject oldObjectData = (JObject)systemFeature.OldValue[index];
                            ulong oldObjectId = Convert.ToUInt64(oldObjectData.GetValue("id"));

                            if ((JObject)systemFeature.NewValue[index] != null)
                            {
                                UpdateSystemPolicyData? input = ((JObject)systemFeature.NewValue[index]).ToObject<UpdateSystemPolicyData>();
                                if (input != null)
                                {
                                    input.Id = oldObjectId;
                                    ServiceResponse<List<UpdatedResponse>> res = await _systemFeatureBL.UpdateSystemPolicyByIdAsync(systemFeature.Id, [input], false, true);
                                    errors.Add(res);
                                }
                            }
                        }
                    }
                }
            }

            return errors.Where(x => x.StatusCode != HttpStatusCode.OK).ToList();
        }


        private async Task<List<ApiResponse>> SaveUserData(UserUnsavedChanges data)
        {
            List<ApiResponse> errors = new List<ApiResponse>();

            if (data != null)
            {
                if (data.Create.Any())
                {
                    foreach (object user in data.Create)
                    {
                        if ((JObject)user != null)
                        {
                            CreateUser? input = ((JObject)user).ToObject<CreateUser>();
                            if (input != null)
                            {
                                ServiceResponse<User> res = await _userBL.AddUserAsync(input, true);
                                errors.Add(res);
                            }
                        }
                    }
                }
                if (data.Update.Any())
                {
                    foreach (UpdateData user in data.Update)
                    {
                        if ((JObject)user.NewValue != null)
                        {
                            UpdateUser? input = ((JObject)user.NewValue).ToObject<UpdateUser>();
                            if (input != null)
                            {
                                ServiceResponse<User> res = await _userBL.UpdateUserAsync(input, user.Id, true);
                                errors.Add(res);
                            }
                        }
                    }
                }
                if (data.Unlock.Any())
                {
                    foreach (UnlockData user in data.Unlock)
                    {
                        ApiResponse res = await _userBL.UnlockUserAsync(user.Id, user.ChangePasswordOnLogin, true);
                        errors.Add(res);
                    }
                }
                if (data.Retire.Any())
                {
                    foreach (RetireData user in data.Retire)
                    {
                        ApiResponse res = await _userBL.RetireUserAsync(user.Id, true);
                        errors.Add(res);
                    }
                }
                if (data.Delete.Any())
                {
                    foreach (DeleteData user in data.Delete)
                    {
                        ApiResponse res = await _userBL.DeleteUserAsync(user.Id, true);
                        errors.Add(res);
                    }
                }
            }

            return errors.Where(x => x.StatusCode != HttpStatusCode.OK).ToList();
        }


        private async Task<List<ApiResponse>> SaveRoleData(UnsavedChanges data)
        {
            List<ApiResponse> errors = new List<ApiResponse>();

            if (data != null)
            {
                if (data.Create.Any())
                {
                    foreach (object role in data.Create)
                    {
                        if ((JObject)role != null)
                        {
                            CreateRole? input = ((JObject)role).ToObject<CreateRole>();
                            if (input != null)
                            {
                                var res = await _roleBL.AddRoleAsync(input, true);
                                errors.Add(new(res.Message, res.StatusCode));
                            }
                        }
                    }
                }
                if (data.Update.Any())
                {
                    foreach (UpdateData role in data.Update)
                    {
                        if ((JObject)role.NewValue != null)
                        {
                            var input = ((JObject)role.NewValue).ToObject<UpdateRole>();
                            if (input != null)
                            {
                                var res = await _roleBL.UpdateRoleAsync(role.Id, input, true);
                                errors.Add(new(res.Message, res.StatusCode));
                            }
                        }
                    }
                }
                if (data.Delete.Any())
                {
                    foreach (DeleteData role in data.Delete)
                    {
                        var res = await _roleBL.DeleteRoleAsync(role.Id, true);
                        errors.Add(new(res.Message, res.StatusCode));
                    }
                }
            }

            return errors.Where(x => x.StatusCode != HttpStatusCode.OK).ToList();
        }

        private async Task<List<ApiResponse>> SaveDeviceData(UnsavedChanges data)
        {
            List<ApiResponse> errors = new List<ApiResponse>();

            if (data != null)
            {
                if (data.Create.Any())
                {
                    foreach (object device in data.Create)
                    {
                        if ((JObject)device != null)
                        {
                            CreateDevice? input = ((JObject)device).ToObject<CreateDevice>();
                            if (input != null)
                            {
                                var res = await _deviceBL.AddDeviceAsync(input, true);
                                errors.Add(new(ResponseConstants.Success, HttpStatusCode.OK));
                            }
                        }
                    }
                }
                if (data.Update.Any())
                {
                    foreach (UpdateData device in data.Update)
                    {
                        if ((JObject)device.NewValue != null)
                        {
                            var input = ((JObject)device.NewValue).ToObject<UpdateDevice>();
                            if (input != null)
                            {
                                var res = await _deviceBL.UpdateDeviceAsync(device.Id, input, true);
                                errors.Add(new(ResponseConstants.Success, HttpStatusCode.OK));
                            }
                        }
                    }
                }
                if (data.Delete.Any())
                {
                    foreach (DeleteData device in data.Delete)
                    {
                        var res = await _deviceBL.DeleteDeviceAsync(device.Id, true);
                        errors.Add(new(ResponseConstants.Success, HttpStatusCode.OK));
                    }
                }
            }

            return errors.Where(x => x.StatusCode != HttpStatusCode.OK).ToList();
        }
    }
}
