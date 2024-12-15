using Newtonsoft.Json;
using static SecMan.Model.ValidateLanguageAttribute;


namespace SecMan.Model
{

    public class LoginRequest
    {
        [JsonProperty("username"), ValidNotNull]
        public string? Username { get; set; }

        [JsonProperty("password"), ValidNotNull]
        public string? Password { get; set; }
    }




    public class LoginResponse
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }
    }


    public class LoginServiceResponse
    {
        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("ssoSessionId")]
        public string? SSOSessionId { get; set; }

        [JsonProperty("expiresIn")]
        public double ExpiresIn { get; set; }

        [JsonProperty("isPasswordExpired")]
        public bool? IsPasswordExpired { get; set; }
    }



    public class JwtTokenOptions
    {
        [JsonProperty("jwtTokenValue")]
        public const string JWTTokenValue = "JWT";

        [JsonProperty("validIssuer")]
        public string? ValidIssuer { get; set; }

        [JsonProperty("validAudience")]
        public string? ValidAudience { get; set; }

        [JsonProperty("secretKey")]
        public string SecretKey { get; set; } = string.Empty;

        [JsonProperty("tokenExpireTime")]
        public double TokenExpireTime { get; set; }
    }


    public class UserDetails
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("domain")]
        public string? Domain { get; set; }

        [JsonProperty("firstName")]
        public string? FirstName { get; set; }

        [JsonProperty("lastName")]
        public string? LastName { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isRetired")]
        public bool IsRetired { get; set; }

        [JsonProperty("isExpired")]
        public bool IsExpired { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("isLegacy")]
        public bool IsLegacy { get; set; }

        [JsonProperty("roles")]
        public List<string?>? Roles { get; set; }

        [JsonProperty("firstLogin")]
        public bool FirstLogin { get; set; }

        [JsonProperty("custom_attributes")]
        public List<CustomAttributes> Custom_attributes { get; set; } = [];
    }

    public class CustomAttributes
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("value")]
        public string? Value { get; set; }
    }

    public class AppPermissions
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("permission")]
        public List<Permission>? Permissions { get; set; }
    }

    public class Permission
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("signatures")]
        public Signatures? Signatures { get; set; }
    }

    public class Signatures
    {
        [JsonProperty("signature")]
        public bool Signature { get; set; }

        [JsonProperty("authorization")]
        public bool Authorization { get; set; }

        [JsonProperty("note")]
        public bool Note { get; set; }
    }



}
