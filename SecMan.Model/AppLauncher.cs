using System.Text.Json.Serialization;

namespace SecMan.Model
{
    public class Applications
    {
        [JsonPropertyName("Vers")]
        public float Version { get; set; }
        [JsonPropertyName("Apps")]
        public List<ApplicationLauncher>? InstalledApps { get; set; }
    }

    public class ApplicationLauncher
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
        [JsonPropertyName("InstalledOn")]
        public DateTime InstalledOn { get; set; }
    }


    public class ApplicationLauncherResponse
    {
        public float? Version { get; set; }
        public List<string> InstalledApps { get; set; } = new List<string>();

    }
}
