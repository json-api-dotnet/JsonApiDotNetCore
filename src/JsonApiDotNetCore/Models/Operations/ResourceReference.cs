using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.Operations
{
    public class ResourceReference : ResourceIdentifierObject
    {
        [JsonProperty("relationship")]
        public string Relationship { get; set; }
    }
}
