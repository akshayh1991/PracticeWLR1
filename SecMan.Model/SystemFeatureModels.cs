using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SecMan.Model.ValidateLanguageAttribute;

namespace SecMan.Model
{


    public class SystemPolicies
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("isCommon")]
        public bool IsCommon { get; set; }
        [JsonProperty("testConnection")]
        public bool TestConnection { get; set; }
    }



    public class UpdateSystemPolicyData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }



    public class SystemPolicyData
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("value")]
        public string? Value { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("units")]
        public string? Units { get; set; }

        [JsonProperty("minimumValue")]
        public ulong? MinimumValue { get; set; }

        [JsonProperty("maximumValue")]
        public ulong? MaximumValue { get; set; }
    }


}
