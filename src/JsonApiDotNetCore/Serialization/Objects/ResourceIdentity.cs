using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// Shared identity information for various JSON:API objects.
/// </summary>
[PublicAPI]
public abstract class ResourceIdentity
{
    [JsonPropertyName("type")]
    [JsonPropertyOrder(-4)]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    [JsonPropertyOrder(-3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("lid")]
    [JsonPropertyOrder(-2)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Lid { get; set; }

    [JsonPropertyName("version")]
    [JsonPropertyOrder(-1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version { get; set; }
}
