using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.Links
{
    /// <summary>
    /// see links section in https://jsonapi.org/format/#document-top-level
    /// </summary>
    public class TopLevelLinks
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("prev")]
        public string Prev { get; set; }
        
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        // http://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
        public bool ShouldSerializeSelf()
        {
            return (!string.IsNullOrEmpty(Self));
        }

        public bool ShouldSerializeFirst()
        {
            return (!string.IsNullOrEmpty(First));
        }

        public bool ShouldSerializeNext()
        {
            return (!string.IsNullOrEmpty(Next));
        }

        public bool ShouldSerializePrev()
        {
            return (!string.IsNullOrEmpty(Prev));
        }

        public bool ShouldSerializeLast()
        {
            return (!string.IsNullOrEmpty(Last));
        }
    }
}
