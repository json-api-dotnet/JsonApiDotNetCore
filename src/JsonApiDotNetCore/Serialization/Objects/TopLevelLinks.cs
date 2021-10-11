#nullable disable

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "links" in https://jsonapi.org/format/1.1/#document-top-level.
    /// </summary>
    [PublicAPI]
    public sealed class TopLevelLinks
    {
        [JsonPropertyName("self")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Self { get; set; }

        [JsonPropertyName("related")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Related { get; set; }

        [JsonPropertyName("describedby")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DescribedBy { get; set; }

        [JsonPropertyName("first")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string First { get; set; }

        [JsonPropertyName("last")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Last { get; set; }

        [JsonPropertyName("prev")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Prev { get; set; }

        [JsonPropertyName("next")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Next { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self) || !string.IsNullOrEmpty(Related) || !string.IsNullOrEmpty(DescribedBy) || !string.IsNullOrEmpty(First) ||
                !string.IsNullOrEmpty(Last) || !string.IsNullOrEmpty(Prev) || !string.IsNullOrEmpty(Next);
        }
    }
}
