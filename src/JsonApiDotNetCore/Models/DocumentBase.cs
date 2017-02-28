using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class DocumentBase
    {
        [JsonProperty("included")]
        public List<DocumentData> Included { get; set; }

        [JsonProperty("meta")]
        public Dictionary<string, object> Meta { get; set; }

        // http://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
        public bool ShouldSerializeIncluded()
        {
            return (Included != null);
        }

        public bool ShouldSerializeMeta()
        {
            return (Meta != null);
        }
    }
}
