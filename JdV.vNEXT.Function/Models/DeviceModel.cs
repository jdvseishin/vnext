using Newtonsoft.Json;

namespace JdV.vNEXT.Function.Models
{
    public class DeviceModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}