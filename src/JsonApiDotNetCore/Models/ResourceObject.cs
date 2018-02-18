using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class ResourceObject : ResourceIdentifierObject
    {
        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, RelationshipData> Relationships { get; set; }
    }
}
