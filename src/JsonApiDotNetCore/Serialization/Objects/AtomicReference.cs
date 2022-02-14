using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See "ref" in https://jsonapi.org/ext/atomic/#operation-objects.
/// </summary>
[PublicAPI]
public sealed class AtomicReference : IResourceIdentity
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

    [JsonPropertyName("relationship")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Relationship { get; set; }
}
