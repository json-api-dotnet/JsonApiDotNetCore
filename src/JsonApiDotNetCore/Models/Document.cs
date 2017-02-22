using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Document
    {
        [JsonProperty("data")]
        public DocumentData Data { get; set; }
        
        [JsonProperty("included")]
        public List<DocumentData> Included { get; set; }
    }
}
