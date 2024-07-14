using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceLinks
{
    [JsonPropertyName("self")]
    public string Self { get; set; } = null!;
}
