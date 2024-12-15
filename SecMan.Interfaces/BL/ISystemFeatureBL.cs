using Microsoft.AspNetCore.Mvc.ModelBinding;
using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface ISystemFeatureBL
    {
        Task<ServiceResponse<List<SystemPolicies>>> GetSystemPoliciesAsync(bool isCommon);

        Task<ServiceResponse<List<SystemPolicyData>>> GetSystemPolicyByIdAsync(ulong id, bool isCommon);

        Task<ServiceResponse<List<UpdatedResponse>>> UpdateSystemPolicyByIdAsync(ulong id, List<UpdateSystemPolicyData> model, bool isCommon, bool saveToDb = false, ModelStateDictionary? modelState = null);
    }
}
