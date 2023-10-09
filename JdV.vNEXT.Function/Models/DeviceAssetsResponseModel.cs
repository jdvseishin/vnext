using Newtonsoft.Json;
using System.Collections.Generic;

namespace JdV.vNEXT.Function.Models
{
    public class DeviceAssetsResponseModel
    {
        [JsonProperty("devices")]
        public IEnumerable<DeviceAssetModel> Devices { get; set; }
    }
}