using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "ref" in https://jsonapi.org/ext/atomic/#operation-objects.
    /// </summary>
    public sealed class AtomicReference : ResourceIdentifierObject
    {
        [JsonProperty("relationship", NullValueHandling = NullValueHandling.Ignore)]
        public string Relationship { get; set; }
    }
}
