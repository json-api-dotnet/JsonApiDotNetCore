using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class DocumentData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }
        
        [JsonProperty("relationships")]
        public Dictionary<string, RelationshipData> Relationships { get; set; }
    }
}
