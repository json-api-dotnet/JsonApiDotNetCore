using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/ext/atomic/#operation-objects.
    /// </summary>
    public sealed class AtomicOperationObject : ExposableData<ResourceObject>
    {
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        [JsonProperty("op")]
        public AtomicOperationCode Code { get; set; }

        [JsonProperty("ref", NullValueHandling = NullValueHandling.Ignore)]
        public AtomicReference Ref { get; set; }

        [JsonProperty("href", NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; set; }
    }
}
