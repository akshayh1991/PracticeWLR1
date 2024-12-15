using Newtonsoft.Json;

namespace SecMan.Model
{
    public class UsersJsonData
    {
        [JsonProperty("users")]
        public UserUnsavedChanges Users { get; set; } = new UserUnsavedChanges();
    }

    public class RolesJsonData
    {
        [JsonProperty("roles")]
        public UnsavedChanges Roles { get; set; } = new UnsavedChanges();
    }


    public class SystemFeatureJsonData
    {
        [JsonProperty("settings")]
        public SystemFeaturesUnSavedChanges Settings { get; set; } = new SystemFeaturesUnSavedChanges();
    }

    public class DeviceJsonData
    {
        [JsonProperty("devices")]
        public UnsavedChanges Devices { get; set; } = new UnsavedChanges();
    }


    public class UnsavedChanges
    {
        [JsonProperty("create")]
        public List<object>? Create { get; set; } = new List<object> { };

        [JsonProperty("update")]
        public List<UpdateData>? Update { get; set; } = new List<UpdateData> { };

        [JsonProperty("delete")]
        public List<DeleteData>? Delete { get; set; } = new List<DeleteData> { };
    }


    public class SystemFeaturesUnSavedChanges : UnsavedChanges
    {
        public SystemFeaturesUnSavedChanges()
        {
            Create = null;
            Delete = null;
        }

        [JsonProperty("update")]
        public List<SystemFeaturesUpdateData>? Update { get; set; } = new List<SystemFeaturesUpdateData> { };
    }
    //public class DevicesUnSavedChanges : UnsavedChanges
    //{
    //    public DevicesUnSavedChanges()
    //    {
    //        Create = null;
    //        Delete = null;
    //    }

    //    [JsonProperty("update")]
    //    public List<DevicesUpdateData>? Update { get; set; } = new List<DevicesUpdateData> { };
    //}

    public class UserUnsavedChanges : UnsavedChanges
    {
        [JsonProperty("retire")]
        public List<RetireData> Retire { get; set; } = new List<RetireData> { };

        [JsonProperty("unlock")]
        public List<UnlockData> Unlock { get; set; } = new List<UnlockData> { };

    }


    public class DeleteData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class RetireData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }


    public class UnlockData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("changePasswordOnLogin")]
        public bool ChangePasswordOnLogin { get; set; }
    }



    public class UpdateData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("oldValue")]
        public object OldValue { get; set; }

        [JsonProperty("newValue")]
        public object NewValue { get; set; }
    }


    public class SystemFeaturesUpdateData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("oldValue")]
        public List<object> OldValue { get; set; }

        [JsonProperty("newValue")]
        public List<object> NewValue { get; set; }
    }

    //public class DevicesUpdateData
    //{
    //    [JsonProperty("id")]
    //    public ulong Id { get; set; }

    //    [JsonProperty("name")]
    //    public string Name { get; set; }

    //    [JsonProperty("oldValue")]
    //    public List<object> OldValue { get; set; }

    //    [JsonProperty("newValue")]
    //    public List<object> NewValue { get; set; }
    //}
}
