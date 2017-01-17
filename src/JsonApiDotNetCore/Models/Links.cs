using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Links
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("related")]
        public string Related { get; set; }
    }
}
