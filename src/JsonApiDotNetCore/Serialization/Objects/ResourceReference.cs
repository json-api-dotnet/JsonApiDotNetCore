using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public class ResourceReference : ResourceIdentifierObject
    {
        [JsonProperty("relationship")]
        public string Relationship { get; set; }
    }
}
