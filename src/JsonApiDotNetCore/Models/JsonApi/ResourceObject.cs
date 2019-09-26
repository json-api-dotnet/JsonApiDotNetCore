using System.Collections.Generic;
using JsonApiDotNetCore.Models.Links;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{ 
    public class ResourceObject : ResourceIdentifierObject
    {
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, RelationshipData> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public ResourceLinks Links { get; set; }
    }
}
