using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class DocumentBase
    {        
        [JsonProperty("included")]
        public List<DocumentData> Included { get; set; }
    }
}
