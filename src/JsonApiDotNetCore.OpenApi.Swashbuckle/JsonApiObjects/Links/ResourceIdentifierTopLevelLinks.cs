using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceIdentifierTopLevelLinks
{
    [JsonPropertyName("self")]
    public required string Self { get; set; }

    [JsonPropertyName("related")]
    public required string Related { get; set; }

    [JsonPropertyName("describedby")]
    public required string Describedby { get; set; }
}
