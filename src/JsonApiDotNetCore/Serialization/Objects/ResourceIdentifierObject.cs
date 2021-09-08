using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-resource-identifier-objects.
    /// </summary>
    public class ResourceIdentifierObject
    {
        [JsonProperty("type", Order = -4)]
        public string Type { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore, Order = -3)]
        public string Id { get; set; }

        [JsonProperty("lid", NullValueHandling = NullValueHandling.Ignore, Order = -2)]
        public string Lid { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
