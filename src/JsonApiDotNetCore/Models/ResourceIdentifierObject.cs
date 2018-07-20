using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class ResourceIdentifierObject
    {
        public ResourceIdentifierObject() { }
        public ResourceIdentifierObject(string type, string id)
        {
            Type = type;
            Id = id;
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lid")]
        public string LocalId { get; set; }
    }
}
