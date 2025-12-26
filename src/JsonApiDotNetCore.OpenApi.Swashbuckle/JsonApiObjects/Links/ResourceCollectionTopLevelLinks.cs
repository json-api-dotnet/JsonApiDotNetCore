using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceCollectionTopLevelLinks
{
    [JsonPropertyName("self")]
    public required string Self { get; set; }

    [JsonPropertyName("describedby")]
    public required string Describedby { get; set; }

    [JsonPropertyName("first")]
    public required string First { get; set; }

    [JsonPropertyName("last")]
    public required string Last { get; set; }

    [JsonPropertyName("prev")]
    public required string Prev { get; set; }

    [JsonPropertyName("next")]
    public required string Next { get; set; }
}
