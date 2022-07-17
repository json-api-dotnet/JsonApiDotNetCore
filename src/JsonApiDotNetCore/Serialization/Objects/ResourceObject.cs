using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See https://jsonapi.org/format/1.1/#document-resource-objects.
/// </summary>
[PublicAPI]
public sealed class ResourceObject : ResourceIdentifierObject
{
    [JsonPropertyName("attributes")]
    [JsonPropertyOrder(1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, object?>? Attributes { get; set; }

    [JsonPropertyName("relationships")]
    [JsonPropertyOrder(2)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, RelationshipObject?>? Relationships { get; set; }

    [JsonPropertyName("links")]
    [JsonPropertyOrder(3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResourceLinks? Links { get; set; }
}
