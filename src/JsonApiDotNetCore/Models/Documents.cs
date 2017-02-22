using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Documents
    {
        [JsonProperty("data")]
        public List<DocumentData> Data { get; set; }

        [JsonProperty("included")]
        public List<DocumentData> Included { get; set; }
    }
}
