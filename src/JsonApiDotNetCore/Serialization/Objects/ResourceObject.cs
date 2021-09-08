using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-resource-objects.
    /// </summary>
    public sealed class ResourceObject : ResourceIdentifierObject
    {
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, RelationshipObject> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public ResourceLinks Links { get; set; }
    }
}
