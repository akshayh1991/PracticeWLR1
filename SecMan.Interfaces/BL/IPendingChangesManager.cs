using SecMan.BL.Common;
using SecMan.Model;

namespace SecMan.BL
{
    public interface IPendingChangesManager
    {
        Task<ApiResponse> AddToSessionJsonAsync(object data, JsonEntities entity);
        Task DeleteToSessionJsonAsync(JsonEntities entity, ulong id, string name);
        Dictionary<string, object> GetChangedProperties<T>(T oldObject, T newObject);
        Task RetireToSessionJsonAsync(JsonEntities entity, ulong id, string name);
        Task UnlockToSessionJsonAsync(JsonEntities entity, ulong id, string name, bool changePasswordOnLogin);
        Task<ApiResponse> UpdateToSessionJsonAsync(object data, JsonEntities entity, object originalEntity, ulong id, string name);
    }
}