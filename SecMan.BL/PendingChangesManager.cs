using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SecMan.BL.Common;
using SecMan.Model;
using System.Collections;
using System.IO.Abstractions;
using System.Net;
using System.Reflection;

namespace SecMan.BL
{
    public class PendingChangesManager : IPendingChangesManager
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IFileSystem _fileSystem;

        public PendingChangesManager(IHttpContextAccessor httpContext,
                              IConfiguration configuration,
                              IFileSystem fileSystem)
        {
            _httpContext = httpContext;
            _configuration = configuration;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// This Method will return the newly passed values as Dictionary in response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldObject"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetChangedProperties<T>(T oldObject, T newObject)
        {
            Dictionary<string, object> changedProperties = new Dictionary<string, object>();

            if (!Equals(newObject, null))
            {
                PropertyInfo[] properties = newObject.GetType().GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    object? oldValue = property.GetValue(oldObject);
                    object? newValue = property.GetValue(newObject);

                    // if updated value is null, skipping that record for update.
                    if (newValue == null)
                    {
                        continue;
                    }

                    string jsonPropertyName = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? property.Name;

                    if (oldValue is IEnumerable oldEnumerable && newValue is IEnumerable newEnumerable)
                    {
                        // Handles comparisons for enumerable properties (e.g., lists).
                        HashSet<object> oldSet = oldEnumerable.Cast<object>().ToHashSet();
                        HashSet<object> newSet = newEnumerable.Cast<object>().ToHashSet();

                        changedProperties[jsonPropertyName] = newValue;
                    }
                    else
                    {
                        changedProperties[jsonPropertyName] = newValue;
                    }
                }
            }

            return changedProperties;
        }


        private bool ConcatenateUpdateObjects<T>(T jsonData, UpdateData updateData, string comparePropertyName = null) where T : UnsavedChanges
        {
            int existingIndex = -1;
            int existingAddIndex = -1;
            var dataExceptCurrentData = jsonData.Update.Where(x => x.Id != updateData.Id);
            if (((JObject)updateData.NewValue).GetValue(comparePropertyName)?.ToString() !=null)
            {
                 existingIndex = dataExceptCurrentData.ToList().FindIndex(d =>
                                      ((JObject)d.NewValue)?.GetValue(comparePropertyName)?.ToString() ==
                                      ((JObject)updateData.NewValue).GetValue(comparePropertyName)?.ToString()
                                      );

                 existingAddIndex = jsonData.Create.ToList().FindIndex(d =>
                          ((JObject)d)?.GetValue(comparePropertyName)?.ToString() ==
                          ((JObject)updateData.NewValue).GetValue(comparePropertyName)?.ToString()
                          );
            }
            if (existingIndex != -1)
            {
                return false;
            }
            else if(existingAddIndex != -1)
            {
                return false;
            }
            else
            {
                // Combines update data with existing unsaved changes if applicable.
                Dictionary<string, object> newData = ConvertToDictionary(updateData.NewValue);
                Dictionary<string, object> oldData = ConvertToDictionary(updateData.OldValue);

                if (newData == null || newData.Count == 0)
                    return true;

                UpdateData? existingData = jsonData?.Update?.Find(a => a.Id == updateData.Id);

                if (existingData != null)
                {
                    // Merges existing data with new updates.
                    Dictionary<string, object> existingNewData = ConvertToDictionary(existingData.NewValue);
                    Dictionary<string, object> existingOldData = ConvertToDictionary(existingData.OldValue);

                    UpdateExistingData(existingNewData, existingOldData, newData);

                    if (existingNewData.Count == 0)
                    {
                        // Removes the update object if there are no differences.
                        jsonData.Update?.Remove(existingData);
                        AppendHeader(ResponseHeaders.ObjectRevert, "true");
                    }
                    else
                    {
                        existingData.NewValue = existingNewData;
                    }
                }
                else
                {
                    // Adds new update object if no existing data matches.
                    Dictionary<string, object> newEntryData = new Dictionary<string, object>();
                    UpdateExistingData(newEntryData, oldData, newData);

                    if (newEntryData.Count > 0)
                    {
                        updateData.NewValue = newEntryData;
                        jsonData.Update?.Add(updateData);
                    }
                }
                return true;
            }
        }


        private static Dictionary<string, object> ConvertToDictionary(object value)
        {
            // Convert Any given object into Dictionary
            return value is JObject jObject ? jObject.ToObject<Dictionary<string, object>>() : new Dictionary<string, object>();
        }


        private void UpdateExistingData(Dictionary<string, object> existingNewData, Dictionary<string, object> existingOldData, Dictionary<string, object> newData)
        {
            foreach (string key in newData.Keys)
            {
                if (AreValuesEquivalent(existingOldData[key], newData[key]))
                {
                    if (existingNewData.Remove(key))
                        AppendHeader(ResponseHeaders.FieldRevert, "true");
                }
                else
                {
                    existingNewData[key] = newData[key];
                }
            }
        }


        private void AppendHeader(string headerName, string headerValue)
        {
            // Adds a custom header to the HTTP response if it doesn't already exist.
            if (!_httpContext.HttpContext.Response.Headers.Any(x => x.Key == headerName))
            {
                _httpContext.HttpContext.Response.Headers.Append(headerName, headerValue);
            }
        }


        private void AddSystemFeaturesUpdateObjects(SystemFeaturesUnSavedChanges jsonData, UpdateData updateData)
        {
            // This is a custom method written to handle Security Policy Logic where changes are saved as array instead of objects
            SystemFeaturesUpdateData? existingData = jsonData.Update.FirstOrDefault(a => a.Id == updateData.Id);

            if (existingData != null)
            {
                ProcessExistingSystemFeature(jsonData, existingData, updateData);
            }
            else
            {
                AddNewSystemFeature(jsonData, updateData);
            }
        }


        private void ProcessExistingSystemFeature(SystemFeaturesUnSavedChanges jsonData, SystemFeaturesUpdateData existingData, UpdateData updateData)
        {
            List<(object OldValue, object NewValue)> itemsToAdd = new List<(object OldValue, object NewValue)>();
            List<int> itemsToRemove = new List<int>();
            bool isUpdated = false;

            for (int i = 0; i < existingData.OldValue.Count; i++)
            {
                JObject oldData = (JObject)existingData.OldValue[i];
                ulong oldObjectId = Convert.ToUInt64(oldData["id"]);
                ulong newObjectId = GetObjectId(updateData.OldValue);

                if (oldObjectId == newObjectId)
                {
                    isUpdated = UpdateMatchingSystemFeature(existingData, updateData, i, itemsToRemove);
                    break;
                }
            }

            if (!isUpdated)
            {
                FindAndAddDifferingValues(updateData, itemsToAdd);
            }

            RemoveSystemFeatures(existingData, itemsToRemove);
            AddSystemFeatures(existingData, itemsToAdd);

            if (existingData.OldValue.Count == 0)
            {
                jsonData.Update?.Remove(existingData);
                AppendHeader(ResponseHeaders.ObjectRevert, "true");
            }
        }


        private void FindAndAddDifferingValues(UpdateData updateData, List<(object OldValue, object NewValue)> itemsToAdd)
        {
            Dictionary<string, object> differingValues = GetDifferingValues(updateData.OldValue, updateData.NewValue);

            // Ensure `id` and `name` are included in the new object.
            differingValues["id"] = GetObjectId(updateData.OldValue);
            differingValues["name"] = GetObjectName(updateData.OldValue);

            if (differingValues.Count > 2) // Check if there are fields besides `id` and `name`.
            {
                itemsToAdd.Add((updateData.OldValue, differingValues));
            }
        }


        private void RemoveSystemFeatures(SystemFeaturesUpdateData existingData, List<int> itemsToRemove)
        {
            // Remove entries in reverse order to avoid index shifting issues.
            foreach (int index in itemsToRemove.OrderByDescending(i => i))
            {
                existingData.OldValue.RemoveAt(index);
                existingData.NewValue.RemoveAt(index);
            }
        }


        private void AddSystemFeatures(SystemFeaturesUpdateData existingData, List<(object OldValue, object NewValue)> itemsToAdd)
        {
            foreach ((object OldValue, object NewValue) item in itemsToAdd)
            {
                existingData.OldValue.Add(item.OldValue);
                existingData.NewValue.Add(item.NewValue);
            }
        }


        private void AddNewSystemFeature(SystemFeaturesUnSavedChanges jsonData, UpdateData updateData)
        {
            Dictionary<string, object> differingNewValues = GetDifferingValues(updateData.OldValue, updateData.NewValue);

            // Ensure id and name are included in the new object.
            differingNewValues["id"] = GetObjectId(updateData.OldValue);
            differingNewValues["name"] = GetObjectName(updateData.OldValue);

            if (differingNewValues.Count > 2) // Check if there are fields besides `id` and `name`.
            {
                jsonData.Update.Add(new SystemFeaturesUpdateData
                {
                    Id = updateData.Id,
                    Name = updateData.Name,
                    OldValue = new List<object> { updateData.OldValue },
                    NewValue = new List<object> { differingNewValues }
                });
            }
        }


        private bool UpdateMatchingSystemFeature(SystemFeaturesUpdateData existingData, UpdateData updateData, int index, List<int> itemsToRemove)
        {
            Dictionary<string, object> oldData = ((JObject)existingData.OldValue[index]).ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
            Dictionary<string, object> newData = updateData.NewValue as Dictionary<string, object> ?? new Dictionary<string, object>();

            // Ensure id and name are included in the new object.
            newData["id"] = GetObjectId(updateData.OldValue);
            newData["name"] = GetObjectName(updateData.OldValue);

            UpdateValues(oldData, newData);

            if (newData.Count > 2) // Check if there are fields besides `id` and `name`.
            {
                existingData.NewValue[index] = newData;
                return true;
            }

            itemsToRemove.Add(index);
            AppendHeader(ResponseHeaders.FieldRevert, "true");
            return true;
        }


        private Dictionary<string, object> GetDifferingValues(object oldObject, object newObject)
        {
            Dictionary<string, object> differingValues = new Dictionary<string, object>();
            Dictionary<string, object?> oldValues = oldObject.GetType().GetProperties().ToDictionary(p => p.Name.ToLower(), p => p.GetValue(oldObject));
            Dictionary<string, object> newValues = newObject as Dictionary<string, object> ?? new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> newKeyValue in newValues)
            {
                object? oldValue = oldValues.ContainsKey(newKeyValue.Key.ToLower()) ? oldValues[newKeyValue.Key.ToLower()] : null;

                if (!AreValuesEquivalent(oldValue, newKeyValue.Value))
                {
                    differingValues[newKeyValue.Key] = newKeyValue.Value;
                }
            }

            return differingValues;
        }


        private void UpdateValues(Dictionary<string, object> oldValues, Dictionary<string, object> newValues)
        {
            var key = "value";
            if (oldValues.TryGetValue(key, out object? oldValue) &&
                AreValuesEquivalent(oldValue, newValues[key]))
            {
                newValues.Remove(key);
                AppendHeader(ResponseHeaders.FieldRevert, "true");
            }
        }


        private bool AreValuesEquivalent(object oldValue, object newValue)
        {
            if (oldValue is string oldString && newValue is string newString)
            {
                return oldString == newString;
            }

            if (oldValue is IEnumerable oldEnumerable && newValue is IEnumerable newEnumerable)
            {
                HashSet<object> oldSet = oldEnumerable.Cast<object>().ToHashSet();
                HashSet<object> newSet = newEnumerable.Cast<object>().ToHashSet();
                return oldSet.SetEquals(newSet);
            }

            return Equals(oldValue, newValue);
        }



        private ulong GetObjectId(object obj)
        {
            return Convert.ToUInt64(obj.GetType().GetProperty("Id")?.GetValue(obj));
        }


        private string? GetObjectName(object obj)
        {
            return Convert.ToString(obj.GetType().GetProperty("Name")?.GetValue(obj));
        }


        private static void ConcatenateDeleteObjects<T>(T jsonData, DeleteData x) where T : UnsavedChanges
        {
            if (!jsonData.Delete.Exists(a => a.Id == x.Id))
            {
                jsonData.Delete.Add(x);
            }
        }


        private static void ConcatenateUserRetireObjects(UserUnsavedChanges jsonData, RetireData x)
        {
            if (!jsonData.Retire.Exists(a => a.Id == x.Id))
            {
                jsonData.Retire.Add(x);
            }
        }


        private static void ConcatenateUserUnlockObjects(UserUnsavedChanges jsonData, UnlockData x)
        {
            if (!jsonData.Unlock.Exists(a => a.Id == x.Id))
            {
                jsonData.Unlock.Add(x);
            }
            else
            {
                var data = jsonData.Unlock.Where(a => a.Id == x.Id).FirstOrDefault();
                data.Name = x.Name;
                data.ChangePasswordOnLogin = x.ChangePasswordOnLogin;
            }
        }


        private static bool ConcatenateCreateObjects<T>(T jsonData, object x, string comparePropertyName) where T : UnsavedChanges
        {
            int existingIndex = jsonData.Create.FindIndex(a =>
                                            ((JObject)a)?.GetValue(comparePropertyName)?.ToString() ==
                                            ((JObject)x)?.GetValue(comparePropertyName)?.ToString()
                                        );

            var existingUpdateIndex = jsonData.Update.ToList().FindIndex(d =>
                      ((JObject)d.NewValue)?.GetValue(comparePropertyName)?.ToString() ==
                      ((JObject)x).GetValue(comparePropertyName)?.ToString()
                      );

            if (existingIndex != -1)
            {
                return false;
            }
            else if(existingUpdateIndex != -1)
            {
                return false;
            }
            else
            {
                Dictionary<string, object>? properties = ((JObject)x).ToObject<Dictionary<string, object>>();
                List<string> keysToRemove = new List<string>();
                foreach (KeyValuePair<string, object> item in properties)
                {
                    string? stringValue = Convert.ToString(item.Value);
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        keysToRemove.Add(item.Key);
                    }
                }

                keysToRemove.ForEach(x =>
                {
                    properties.Remove(x);
                });

                jsonData.Create.Add(properties);
                return true;
            }

        }


        private async Task<string> WriteToSessionJson(UnsavedChanges unSavedChanges, JsonEntities entity)
        {
            // Writes unsaved changes to a session-specific JSON file.
            string jsonFileName = $"{_configuration.GetSection("SessionFilesPath").Value}{_httpContext.HttpContext?.User?.FindFirst(ResponseHeaders.SSOSessionId).Value}.json";
            if (!_fileSystem.Directory.Exists(_configuration.GetSection("SessionFilesPath").Value))
            {
                _fileSystem.Directory.CreateDirectory(_configuration.GetSection("SessionFilesPath").Value);
            }


            if (!_fileSystem.File.Exists(jsonFileName))
            {
                await _fileSystem.File.WriteAllTextAsync(jsonFileName, "{}");
            }

            string jsonString = await _fileSystem.File.ReadAllTextAsync(jsonFileName);

            // Generate an de serialized object based on entity type passed.
            object jsonObject = entity switch
            {
                JsonEntities.User => JsonConvert.DeserializeObject<UsersJsonData>(jsonString) ?? new UsersJsonData(),
                JsonEntities.Role => JsonConvert.DeserializeObject<RolesJsonData>(jsonString) ?? new RolesJsonData(),
                JsonEntities.SystemFeature => JsonConvert.DeserializeObject<SystemFeatureJsonData>(jsonString) ?? new SystemFeatureJsonData(),
                JsonEntities.Device=>JsonConvert.DeserializeObject<DeviceJsonData>(jsonString) ?? new DeviceJsonData(),
                _ => throw new ArgumentException($"Unsupported entity type: {entity}")
            };


            // call custom methods to hands different operations.
            if (entity == JsonEntities.User && jsonObject is UsersJsonData userJsonData)
            {
                UserUnsavedChanges? userUnSavedChanges = JsonConvert.DeserializeObject<UserUnsavedChanges>(
                    JsonConvert.SerializeObject(unSavedChanges)
                    );
                if (userUnSavedChanges != null)
                {
                    foreach (object x in userUnSavedChanges.Create)
                    {
                        bool status = ConcatenateCreateObjects(userJsonData.Users, x, "username");
                        if (!status)
                            return ResponseConstants.UserAlreadyExists;
                    }
                    foreach (UpdateData x in userUnSavedChanges.Update)
                    {
                        bool status = ConcatenateUpdateObjects(userJsonData.Users, x, "username");
                        if (!status)
                            return ResponseConstants.UserAlreadyExists;
                    };
                    userUnSavedChanges.Delete.ForEach(x =>
                    {
                        ConcatenateDeleteObjects(userJsonData.Users, x);
                    });
                    userUnSavedChanges.Retire.ForEach(x =>
                    {
                        ConcatenateUserRetireObjects(userJsonData.Users, x);
                    });
                    userUnSavedChanges.Unlock.ForEach(x =>
                    {
                        ConcatenateUserUnlockObjects(userJsonData.Users, x);
                    });
                }
            }
            else if (entity == JsonEntities.SystemFeature && jsonObject is SystemFeatureJsonData systemFeatureJsonData)
            {
                unSavedChanges?.Update.ForEach(x =>
                {
                    AddSystemFeaturesUpdateObjects(systemFeatureJsonData.Settings, x);
                });
            }
            else if (entity == JsonEntities.Role && jsonObject is RolesJsonData rolesJsonData)
            {
                UnsavedChanges? roleUnSavedChanges = JsonConvert.DeserializeObject<UnsavedChanges>(
                                            JsonConvert.SerializeObject(unSavedChanges)
                                            );
                if (roleUnSavedChanges != null)
                {
                    foreach (object x in roleUnSavedChanges.Create)
                    {
                        bool status = ConcatenateCreateObjects(rolesJsonData.Roles, x, "name");
                        if (!status)
                            return RoleResponseConstants.RoleAlreadyExists;
                    }
                    foreach (UpdateData x in roleUnSavedChanges.Update)
                    {
                        bool status = ConcatenateUpdateObjects(rolesJsonData.Roles, x, "name");
                        if (!status)
                            return RoleResponseConstants.RoleAlreadyExists;
                    };
                    roleUnSavedChanges.Delete.ForEach(x =>
                    {
                        ConcatenateDeleteObjects(rolesJsonData.Roles, x);
                    });
                }
            }
            else if (entity == JsonEntities.Device && jsonObject is DeviceJsonData devicesJsonData)
            {
                UnsavedChanges? deviceUnSavedChanges = JsonConvert.DeserializeObject<UnsavedChanges>(
                                            JsonConvert.SerializeObject(unSavedChanges)
                                            );
                if (deviceUnSavedChanges != null)
                {
                    foreach (object x in deviceUnSavedChanges.Create)
                    {
                        bool status = ConcatenateCreateObjects(devicesJsonData.Devices, x, "name");
                        if (!status)
                            return DeviceResponseConstants.DeviceAlreadyExists;
                    }
                    foreach (UpdateData x in deviceUnSavedChanges.Update)
                    {
                        bool status = ConcatenateUpdateObjects(devicesJsonData.Devices, x, "name");
                        if (!status)
                            return DeviceResponseConstants.DeviceAlreadyExists;
                    };
                    deviceUnSavedChanges.Delete.ForEach(x =>
                    {
                        ConcatenateDeleteObjects(devicesJsonData.Devices, x);
                    });
                }
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonText = JsonConvert.SerializeObject(jsonObject, settings);

            await _fileSystem.File.WriteAllTextAsync(jsonFileName, jsonText);

            return ResponseConstants.Success;
        }


        public async Task<ApiResponse> AddToSessionJsonAsync(object data, JsonEntities entity)
        {
            UnsavedChanges jsonObject = new UnsavedChanges();
            jsonObject.Create.Add(data);

            string res = await WriteToSessionJson(jsonObject, entity);
            if (res == ResponseConstants.Success)
                return new(ResponseConstants.Success, HttpStatusCode.OK);
            else
                return new(res, HttpStatusCode.Conflict);
        }


        public async Task<ApiResponse> UpdateToSessionJsonAsync(object data, JsonEntities entity, object originalEntity, ulong id, string name)
        {
            UnsavedChanges jsonObject = new UnsavedChanges();

            Dictionary<string, object> changes = GetChangedProperties(originalEntity, data);
            if (changes.Count != 0)
            {
                jsonObject.Update.Add(new UpdateData
                {
                    NewValue = changes,
                    OldValue = originalEntity,
                    Name = name,
                    Id = id,
                });
            }

            string res = await WriteToSessionJson(jsonObject, entity);
            if (res == ResponseConstants.Success)
                return new(ResponseConstants.Success, HttpStatusCode.OK);
            else
                return new(res, HttpStatusCode.Conflict);
        }


        public async Task DeleteToSessionJsonAsync(JsonEntities entity, ulong id, string name)
        {
            UnsavedChanges jsonObject = new UnsavedChanges();

            jsonObject.Delete.Add(new DeleteData
            {
                Name = name,
                Id = id,
            });

            await WriteToSessionJson(jsonObject, entity);
        }


        public async Task RetireToSessionJsonAsync(JsonEntities entity, ulong id, string name)
        {
            UserUnsavedChanges jsonObject = new UserUnsavedChanges();

            jsonObject.Retire.Add(new RetireData
            {
                Name = name,
                Id = id,
            });

            await WriteToSessionJson(jsonObject, entity);
        }


        public async Task UnlockToSessionJsonAsync(JsonEntities entity, ulong id, string name, bool changePasswordOnLogin)
        {
            UserUnsavedChanges jsonObject = new UserUnsavedChanges();

            jsonObject.Unlock.Add(new UnlockData
            {
                Name = name,
                Id = id,
                ChangePasswordOnLogin = changePasswordOnLogin
            });

            await WriteToSessionJson(jsonObject, entity);
        }
    }
}
