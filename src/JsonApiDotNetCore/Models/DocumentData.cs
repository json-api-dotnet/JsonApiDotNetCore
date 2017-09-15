using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class DocumentData
    {
        [JsonProperty("type")]
        public object Type { get; set; }

        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, RelationshipData> Relationships { get; set; }
    }
}