using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Documents : DocumentBase
    {
        [JsonProperty("data")]
        public List<DocumentData> Data { get; set; }
    }
}
