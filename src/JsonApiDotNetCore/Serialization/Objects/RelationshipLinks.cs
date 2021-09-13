using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "links" in https://jsonapi.org/format/1.1/#document-resource-object-relationships.
    /// </summary>
    [PublicAPI]
    public sealed class RelationshipLinks
    {
        [JsonPropertyName("self")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Self { get; set; }

        [JsonPropertyName("related")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Related { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self) || !string.IsNullOrEmpty(Related);
        }
    }
}
