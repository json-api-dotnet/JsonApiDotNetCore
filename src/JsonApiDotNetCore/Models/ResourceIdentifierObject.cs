using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class ResourceIdentifierObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lid")]
        public string LocalId { get; set; }
    }
}
