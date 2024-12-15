using Newtonsoft.Json.Linq;
using SecMan.Model;

namespace SecMan.BL
{
    public interface IReviewerBl
    {
        Task<ServiceResponse<JObject>> ReadJsonData();
        Task<ApiResponse> SaveUnsavedJsonChanges(JObject model);
        Task<ApiResponse> SaveUnsavedChanges(JObject model);
    }
}