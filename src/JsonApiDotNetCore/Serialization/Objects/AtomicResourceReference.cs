using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class AtomicResourceReference : ResourceIdentifierObject
    {
        [JsonProperty("relationship", NullValueHandling = NullValueHandling.Ignore)]
        public string Relationship { get; set; }
    }
}
