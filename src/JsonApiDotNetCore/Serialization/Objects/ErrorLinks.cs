using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See "links" in https://jsonapi.org/format/#error-objects.
/// </summary>
[PublicAPI]
public sealed class ErrorLinks
{
    [JsonPropertyName("about")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? About { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
}
