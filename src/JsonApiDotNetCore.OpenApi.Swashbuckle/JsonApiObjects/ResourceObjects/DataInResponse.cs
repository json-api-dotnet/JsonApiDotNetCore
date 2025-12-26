using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class DataInResponse<TResource> : ResourceInResponse
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("id")]
    public override required string Id { get; set; }

    [JsonPropertyName("attributes")]
    public required AttributesInResponse<TResource> Attributes { get; set; }

    [JsonPropertyName("relationships")]
    public required RelationshipsInResponse<TResource> Relationships { get; set; }

    // Non-required because the related controller may be unavailable when used in an include.
    [JsonPropertyName("links")]
    public required ResourceLinks Links { get; set; }
}
