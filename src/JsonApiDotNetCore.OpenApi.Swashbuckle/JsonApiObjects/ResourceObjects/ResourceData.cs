using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal abstract class ResourceData : IResourceIdentity
{
    [Required]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    public abstract string Id { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
