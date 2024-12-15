using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SecMan.Model
{
    public class CreateRole
    {
        [JsonProperty("name")]
        [Required(ErrorMessage ="Name cannot be empty")]
        [StringLength(120, ErrorMessage = "Name cannot be longer than 32 characters.")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        [StringLength(120, ErrorMessage = "Description cannot be longer than 120 characters.")]
        public string? Description { get; set; }

        [JsonProperty("isLoggedOutType")]
        public bool? IsLoggedOutType { get; set; }

        [JsonProperty("linkUsers")]
        public List<ulong>? LinkUsers { get; set; }
    }


    public class UpdateRole
    {
        [JsonProperty("name")]
        [StringLength(120, ErrorMessage = "Name cannot be longer than 32 characters.")]
        public string? Name { get; set; }

        [StringLength(120, ErrorMessage = "Description cannot be longer than 120 characters.")]
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("isLoggedOutType")]
        public bool? IsLoggedOutType { get; set; }

        [JsonProperty("linkUsers")]
        public List<ulong>? LinkUsers { get; set; }
    }



    public class GetRoleDto
    {
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsLoggedOutType { get; set; }
        public int NoOfUsers { get; set; }
    }
}
