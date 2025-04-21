using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal abstract class ResourceData : IResourceIdentity
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    public abstract string Id { get; set; }

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
