using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Model
{
    public class CreateDevice
    {
        [JsonProperty("name")]
        [Required(ErrorMessage = "Device Name cannot be empty")]
        [StringLength(100, ErrorMessage = "Device Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [JsonProperty("typeId")]
        [Required(ErrorMessage = "Type Id cannot be empty")]
        public ulong? TypeId { get; set; }

        [JsonProperty("ip")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Invalid IP address format.")]
        public string Ip { get; set; }

        [JsonProperty("zoneId")]
        public ulong ZoneId { get; set; }
        [JsonProperty("deploymentStatus")]
        public string DeploymentStatus { get; set; } = "Disabled";
        [JsonProperty("isLegacy")]
        public bool IsLegacy { get; set; } = false;
    }

    public class UpdateDevice
    {
        [JsonProperty("name")]
        [StringLength(100, ErrorMessage = "Device Name cannot exceed 100 characters.")]
        public string? Name { get; set; }
        [JsonProperty("typeId")]
        public ulong? TypeId { get; set; }
        [JsonProperty("ip")]
        public string? Ip { get; set; }
        [JsonProperty("zoneId")]
        public ulong? ZoneId { get; set; }
        [JsonProperty("deploymentStatus")]
        public string? DeploymentStatus { get; set; }
        [JsonProperty("isLegacy")]
        public bool? IsLegacy { get; set; }
    }

    public class GetDevice
    {
        public ulong Id { get; set; }
        public string Name { get; set; }

        public ulong? TypeId { get; set; }

        public string? Ip { get; set; }

        public ulong? ZoneId { get; set; }

        public string? DeploymentStatus { get; set; }

        public bool? IsLegacy { get; set; } = false;
    }
}
