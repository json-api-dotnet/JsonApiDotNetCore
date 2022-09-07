using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See "ref" in https://jsonapi.org/ext/atomic/#operation-objects.
/// </summary>
[PublicAPI]
public sealed class AtomicReference : ResourceIdentity
{
    [JsonPropertyName("relationship")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Relationship { get; set; }
}
