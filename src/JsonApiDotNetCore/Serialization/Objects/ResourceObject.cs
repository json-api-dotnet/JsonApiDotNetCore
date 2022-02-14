using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See https://jsonapi.org/format/1.1/#document-resource-objects.
/// </summary>
[PublicAPI]
public sealed class ResourceObject : IResourceIdentity
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

    [JsonPropertyName("attributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, object?>? Attributes { get; set; }

    [JsonPropertyName("relationships")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, RelationshipObject?>? Relationships { get; set; }

    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResourceLinks? Links { get; set; }

    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, object?>? Meta { get; set; }
}
