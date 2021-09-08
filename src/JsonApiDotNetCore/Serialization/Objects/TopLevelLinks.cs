using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "links" in https://jsonapi.org/format/1.1/#document-top-level.
    /// </summary>
    public sealed class TopLevelLinks
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public string Related { get; set; }

        [JsonProperty("describedby", NullValueHandling = NullValueHandling.Ignore)]
        public string DescribedBy { get; set; }

        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        public string First { get; set; }

        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        public string Last { get; set; }

        [JsonProperty("prev", NullValueHandling = NullValueHandling.Ignore)]
        public string Prev { get; set; }

        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        public string Next { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self) || !string.IsNullOrEmpty(Related) || !string.IsNullOrEmpty(DescribedBy) || !string.IsNullOrEmpty(First) ||
                !string.IsNullOrEmpty(Last) || !string.IsNullOrEmpty(Prev) || !string.IsNullOrEmpty(Next);
        }
    }
}
