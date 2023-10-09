using Newtonsoft.Json;
using System.Collections.Generic;

namespace JdV.vNEXT.Function.Models
{
    public class DeviceRequestModel
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("devices")]
        public IEnumerable<DeviceModel> Devices { get; set; }
    }
}