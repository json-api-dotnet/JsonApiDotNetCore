using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.Operations
{
    public class ResourceReference
    {
        [JsonProperty("type")]
        public object Type { get; set; }

        [JsonProperty("id")]
        public object Id { get; set; }
        
        [JsonProperty("relationship")]
        public string Relationship { get; set; }
    }
}
