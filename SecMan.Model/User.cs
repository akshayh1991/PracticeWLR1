using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static SecMan.Model.ValidateLanguageAttribute;

namespace SecMan.Model
{
    public class BaseEntity
    {
        public int Id { get; set; }
    }

    public class CreateUser
    {
        [JsonProperty("password"), ValidPassword]
        public string? Password { get; set; }

        [JsonProperty("firstLogin")]
        public bool FirstLogin { get; set; } = true;

        [JsonProperty("username"), ValidUsername]
        public string? Username { get; set; }

        [JsonProperty("domain"), ValidateDomain]
        public string? Domain { get; set; } = "local";

        [JsonProperty("firstName"), ValidateMaxLength(32)]
        public string? FirstName { get; set; }

        [JsonProperty("lastName"), ValidateMaxLength(32)]
        public string? LastName { get; set; }

        [JsonProperty("description"), ValidateMaxLength(120)]
        public string? Description { get; set; }

        [JsonProperty("email"), ValidateEmail]
        public string? Email { get; set; }

        [JsonProperty("language"), ValidateLanguage]
        public string? Language { get; set; } = "en";

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isLegacy")]
        public bool IsLegacy { get; set; } = false;

        [JsonProperty("roles")]
        public List<ulong> Roles { get; set; } = [];

        [JsonProperty("isPasswordExpiryEnabled")]
        public bool IsPasswordExpiryEnabled { get; set; }

        [JsonProperty("resetPassword")]
        public bool ResetPassword { get; set; }
    }


    public class UpdateUser
    {
        [JsonProperty("username"), ValidUsername(true)]
        public string? Username { get; set; }

        [JsonProperty("password"), ValidPassword(true)]
        public string? Password { get; set; }

        [JsonProperty("domain"), ValidateDomain(true)]
        public string? Domain { get; set; }

        [JsonProperty("firstName"), ValidateMaxLength(32)]
        public string? FirstName { get; set; }

        [JsonProperty("lastName"), ValidateMaxLength(32)]
        public string? LastName { get; set; }

        [JsonProperty("description"), ValidateMaxLength(120)]
        public string? Description { get; set; }

        [JsonProperty("email"), ValidateEmail]
        public string? Email { get; set; }

        [JsonProperty("language"), ValidateLanguage(true)]
        public string? Language { get; set; }

        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }

        [JsonProperty("isLegacy")]
        public bool? IsLegacy { get; set; }

        [JsonProperty("roles")]
        public List<ulong>? Roles { get; set; }

        [JsonProperty("isPasswordExpiryEnabled")]
        public bool? IsPasswordExpiryEnabled { get; set; }

        [JsonProperty("resetPassword")]
        public bool? ResetPassword { get; set; }

        [JsonProperty("firstLogin")]
        public bool? FirstLogin { get; set; }
    }



    public class RoleModel
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; } = string.Empty;

        [JsonProperty("isLoggedOutType")]
        public bool? IsLoggedOutType { get; set; }

        [JsonProperty("noOfUsers")]
        public int NoOfUsers { get; set; }
    }

    public class UserAttributeDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }


    public class User : UpdateUser
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("inactiveDate")]
        public DateTime? InactiveDate { get; set; }

        [JsonProperty("isRetired")]
        public bool IsRetired { get; set; }

        [JsonProperty("retiredDate")]
        public DateTime RetiredDate { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("lockedDate")]
        public DateTime LockedDate { get; set; }

        [JsonProperty("lockedReason")]
        public string LockedReason { get; set; } = string.Empty;

        [JsonProperty("lastLogin")]
        public DateTime LastLogin { get; set; }

        [JsonProperty("roles")]
        public new List<RoleModel> Roles { get; set; } = [];

        [JsonProperty("passwordExpiryDate")]
        public DateTime? PasswordExpiryDate { get; set; }
    }


    public class UsersFilterDto : IValidatableObject
    {
        [JsonProperty("username")]
        public string? Username { get; set; } = null;

        [JsonProperty("role")]
        public List<string> Role
        {
            get
            {
                if (!_roles.Where(x => x != null).Select(x => x.ToLower().Replace(" ", string.Empty)).Any())
                {
                    return [];
                }
                else
                {
                    return _roles.Where(x => x != null).Select(x => x.ToLower().Replace(" ", string.Empty)).ToList();
                }
            }
            set => _roles = value;
        }
        private List<string> _roles = [];

        [JsonProperty("status")]
        public List<string> Status
        {
            get
            {
                if (!_status.Where(x => x != null).Select(x => x.ToLower().Replace(" ", string.Empty)).Any())
                {
                    return [];
                }
                else
                {
                    return _status.Where(x => x != null).Select(x => x.ToLower().Replace(" ", string.Empty)).ToList();
                }
            }
            set => _status = value;
        }
        private List<string> _status = [];

        [JsonProperty("offset")]
        [ModelBinder(BinderType = typeof(CustomIntBinder))]
        public int? Offset
        {
            get => _offset ?? 0;
            set => _offset = value;
        }

        private int? _offset = 0;

        [JsonProperty("limit")]
        [ModelBinder(BinderType = typeof(CustomIntBinder))]
        public int? Limit
        {
            get => _limit ?? 500;
            set => _limit = value;
        }

        private int? _limit = 500;

        [JsonProperty("isLegacy")]
        [ModelBinder(BinderType = typeof(CustomBoolBinder))]
        public bool? IsLegacy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            HashSet<string> allowedStatuses = new HashSet<string> { "active", "inactive", "retired", "locked" };

            if (Limit < 0)
            {
                yield return new ValidationResult("Invalid limit value", new[] { $"{char.ToLower(nameof(Limit)[0])}{nameof(Limit)[1..]}" });
            }

            if (Offset < 0)
            {
                yield return new ValidationResult("Invalid offset value", new[] { $"{char.ToLower(nameof(Offset)[0])}{nameof(Offset)[1..]}" });
            }

            if (Status.Count > 0 && Status.TrueForAll(x => string.IsNullOrWhiteSpace(x)))
            {
                yield return new ValidationResult("status cannot Not Be Empty", new[] { $"{char.ToLower(nameof(Status)[0])}{nameof(Status)[1..]}" });
            }

            foreach (string status in Status)
            {
                if (!allowedStatuses.Contains(status))
                {
                    yield return new ValidationResult("Invalid status value", new[] { $"{char.ToLower(nameof(Status)[0])}{nameof(Status)[1..]}" });
                }
            }
        }
    }



    public class UsersWithCountDto
    {
        [JsonProperty("users")]
        public List<User> Users { get; set; } = [];

        [JsonProperty("userCount")]
        public int UserCount { get; set; }
    }
}
