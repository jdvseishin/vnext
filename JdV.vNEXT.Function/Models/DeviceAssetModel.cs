using Newtonsoft.Json;

namespace JdV.vNEXT.Function.Models
{
    public class DeviceAssetModel
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }
    }
}