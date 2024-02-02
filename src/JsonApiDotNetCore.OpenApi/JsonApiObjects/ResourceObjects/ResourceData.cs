using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal abstract class ResourceData : IResourceIdentity
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}
