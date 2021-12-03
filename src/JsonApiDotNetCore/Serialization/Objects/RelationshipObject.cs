using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-resource-object-relationships.
    /// </summary>
    [PublicAPI]
    public sealed class RelationshipObject
    {
        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RelationshipLinks? Links { get; set; }

        [JsonPropertyName("data")]
        // JsonIgnoreCondition is determined at runtime by WriteOnlyRelationshipObjectConverter.
        public SingleOrManyData<ResourceIdentifierObject> Data { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object?>? Meta { get; set; }
    }
}
