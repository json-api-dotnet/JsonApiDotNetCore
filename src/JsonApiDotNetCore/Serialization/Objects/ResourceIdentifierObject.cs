using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See https://jsonapi.org/format/#document-resource-identifier-objects.
/// </summary>
[PublicAPI]
public class ResourceIdentifierObject : ResourceIdentity
{
    [JsonPropertyName("meta")]
    [JsonPropertyOrder(100)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, object?>? Meta { get; set; }
}
