using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceTopLevelLinks
{
    [JsonPropertyName("self")]
    public required string Self { get; set; }

    [JsonPropertyName("describedby")]
    public required string Describedby { get; set; }
}
