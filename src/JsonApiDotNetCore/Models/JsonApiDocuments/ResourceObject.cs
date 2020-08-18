using System.Collections.Generic;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public sealed class ResourceObject : ResourceIdentifierObject
    {
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, RelationshipEntry> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public ResourceLinks Links { get; set; }
    }
}
