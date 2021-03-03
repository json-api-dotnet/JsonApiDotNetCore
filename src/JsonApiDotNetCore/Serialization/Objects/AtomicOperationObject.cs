using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/ext/atomic/#operation-objects.
    /// </summary>
    public sealed class AtomicOperationObject : ExposableData<ResourceObject>
    {
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Meta { get; set; }

        [JsonProperty("op")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AtomicOperationCode Code { get; set; }

        [JsonProperty("ref", NullValueHandling = NullValueHandling.Ignore)]
        public AtomicReference Ref { get; set; }

        [JsonProperty("href", NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; set; }
    }
}
