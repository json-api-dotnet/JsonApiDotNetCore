using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See links section in https://jsonapi.org/format/#document-top-level.
    /// </summary>
    public sealed class TopLevelLinks
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("related")]
        public string Related { get; set; }

        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("prev")]
        public string Prev { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        // http://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
        public bool ShouldSerializeSelf() => !string.IsNullOrEmpty(Self);
        public bool ShouldSerializeRelated() => !string.IsNullOrEmpty(Related);
        public bool ShouldSerializeFirst() => !string.IsNullOrEmpty(First);
        public bool ShouldSerializeLast() => !string.IsNullOrEmpty(Last);
        public bool ShouldSerializePrev() => !string.IsNullOrEmpty(Prev);
        public bool ShouldSerializeNext() => !string.IsNullOrEmpty(Next);
    }
}
