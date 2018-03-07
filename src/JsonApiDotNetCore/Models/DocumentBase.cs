using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class DocumentBase
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public RootLinks Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        public List<DocumentData> Included { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Meta { get; set; }
    }
}
