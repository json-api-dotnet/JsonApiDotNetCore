using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See https://jsonapi.org/format/1.1/#document-resource-identifier-objects.
/// </summary>
[PublicAPI]
public sealed class ResourceIdentifierObject : IResourceIdentity
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Type { get; set; }

    [JsonIgnore]
    public string? Id
    {
        get => NumericId == default ? null : NumericId.ToString();
        set => NumericId = value == null ? 0 : int.Parse(value);
    }

    [JsonPropertyName("id")]
    public long NumericId { get; set; }

    [JsonPropertyName("lid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Lid { get; set; }

    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, object?>? Meta { get; set; }
}
