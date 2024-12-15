using SecMan.BL.Common;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SecMan.BL
{
    public class SystemFeatureBL : ISystemFeatureBL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPendingChangesManager _pendingChangesManager;
        private readonly ICurrentUserService _currentUserService;

        public SystemFeatureBL(IUnitOfWork unitOfWork,
                               IPendingChangesManager pendingChangesManager,
                               ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _pendingChangesManager = pendingChangesManager;
            _currentUserService = currentUserService;
        }


        public async Task<ServiceResponse<List<SystemPolicies>>> GetSystemPoliciesAsync(bool isCommon)
        {
            // Check if isCommon is true and return only common ones else return all common and non common features.
            List<SystemPolicies> systemPolicies = (await _unitOfWork.ISystemFeatureRepository
                                            .GetAll(policy => isCommon ? policy.Common == isCommon : policy.Id != 0))
                                            .Select(policy => new SystemPolicies
                                            {
                                                Id = policy.Id,
                                                IsCommon = policy.Common,
                                                Name = policy.Name,
                                                Description = policy.Description,
                                                TestConnection = policy.TestConnection
                                            })
                                            .ToList();

            return new(ResponseConstants.Success, HttpStatusCode.OK, systemPolicies);
        }


        public async Task<ServiceResponse<List<SystemPolicyData>>> GetSystemPolicyByIdAsync(ulong id, bool isCommon)
        {
            Data.SQLCipher.SysFeat? systemPolicy = await _unitOfWork.ISystemFeatureRepository
                                                .GetById(id, policy => policy.SysFeatProps);
            if (systemPolicy is null || (systemPolicy.Common != isCommon && isCommon))
            {
                return new(isCommon ? ResponseConstants.SettingDoesNotExists : ResponseConstants.SecurityPolicyDoesNotExists, HttpStatusCode.NotFound);
            }
            List<SystemPolicyData>? policies = new List<SystemPolicyData>();
            if (string.IsNullOrWhiteSpace(_currentUserService?.UserDetails?.Language))
            {
                policies = systemPolicy.SysFeatProps?
                                       .Select(policy => new SystemPolicyData
                                       {
                                           Description = policy.Desc,
                                           Id = policy.Id,
                                           MaximumValue = policy.ValMax,
                                           MinimumValue = policy.ValMin,
                                           Name = policy.Name,
                                           Type = policy.ValType,
                                           Value = policy.Val,
                                           Units = policy.Units
                                       })
                                       .ToList();
            }
            else
            {
                systemPolicy = await _unitOfWork.ISystemFeatureRepository.LoadLanguageWiseData(systemPolicy);

                policies = systemPolicy.SysFeatProps?
                       .Select(policyProperties => new SystemPolicyData
                       {
                           Description = policyProperties.Langs?.Where(language => language.Code == _currentUserService?.UserDetails?.Language).Select(loadlang => loadlang.Desc).FirstOrDefault() ?? policyProperties.Desc,
                           Id = policyProperties.Id,
                           MaximumValue = policyProperties.ValMax,
                           MinimumValue = policyProperties.ValMin,
                           Name = policyProperties.Langs?.Where(language => language.Code == _currentUserService?.UserDetails?.Language).Select(loadlang => loadlang.Name).FirstOrDefault() ?? policyProperties.Name,
                           Type = policyProperties.ValType,
                           Value = policyProperties.Val,
                           Units = policyProperties.Units
                       })
                       .ToList();
            }
            return new(ResponseConstants.Success, HttpStatusCode.OK, policies);
        }


        public async Task<ServiceResponse<List<UpdatedResponse>>> UpdateSystemPolicyByIdAsync(ulong id, List<UpdateSystemPolicyData> model, bool isCommon, bool saveToDb = false, ModelStateDictionary? modelState = null)
        {
            var response = new List<UpdatedResponse>();
            Data.SQLCipher.SysFeat? systemPolicy = await _unitOfWork.ISystemFeatureRepository
                                    .GetById(id, policy => policy.SysFeatProps);
            if (systemPolicy is null || (systemPolicy.Common != isCommon && isCommon))
            {
                return new(isCommon ? ResponseConstants.SettingDoesNotExists : ResponseConstants.SecurityPolicyDoesNotExists, HttpStatusCode.NotFound);
            }

            foreach (var policyProperty in model)
            {
                var index = model.IndexOf(policyProperty);
                Data.SQLCipher.SysFeatProp? policy = systemPolicy.SysFeatProps?.Find(property => property.Id == policyProperty.Id);

                if (policy is not null)
                {
                    response.Add(new UpdatedResponse { Id = policy.Id });
                    if (modelState != null)
                    {
                        ServiceResponse<List<UpdatedResponse>>? validationErrors = ValidateValue(policy, policyProperty.Value, modelState, index);
                        if (validationErrors != default)
                        {
                            return validationErrors;
                        }
                    }
                    if (modelState == null || modelState.IsValid)
                    {
                        if (saveToDb)
                        {
                            policy.Val = policyProperty.Value ?? policy.Val;
                        }
                        else
                        {
                            UpdateSystemPolicyData originalObject = new UpdateSystemPolicyData
                            {
                                Id = policy.Id,
                                Name = policy.Name,
                                Value = policy.Val,
                            };
                            await _pendingChangesManager.UpdateToSessionJsonAsync(policyProperty, JsonEntities.SystemFeature, originalObject, systemPolicy.Id, systemPolicy.Name);
                        }
                    }
                }
            }
            if (saveToDb)
            {
                _unitOfWork.ISystemFeatureRepository.Update(systemPolicy);
            }
            return new(ResponseConstants.Success, HttpStatusCode.OK, response);
        }



        private static ServiceResponse<List<UpdatedResponse>>? ValidateValue(Data.SQLCipher.SysFeatProp model, string value, ModelStateDictionary modelState, int index)
        {
            if (model.ValType == nameof(SystemPolicyTypes.Email))
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, pattern))
                {
                    modelState.AddModelError($"[{index}].Value", ValidationConstants.InvalidEmail);
                }
            }
            else if (model.ValType == nameof(SystemPolicyTypes.Bool))
            {
                if (!bool.TryParse(value, out _))
                {
                    modelState.AddModelError($"[{index}].Value", ValidationConstants.InvalidBoolean);
                }
            }
            else if (model.ValType == nameof(SystemPolicyTypes.Int))
            {
                if (!ulong.TryParse(value, out ulong intValue))
                {
                    modelState.AddModelError($"[{index}].Value", ValidationConstants.InvalidNumber);
                }
                else if (intValue < model.ValMin)
                {
                    modelState.AddModelError($"[{index}].Value", ValidationConstants.ValueIsLessThanMin);
                }
                else if (intValue > model.ValMax)
                {
                    modelState.AddModelError($"[{index}].Value", ValidationConstants.ValueIsLessThanMax);
                }
            }
            return default;
        }
    }
}
