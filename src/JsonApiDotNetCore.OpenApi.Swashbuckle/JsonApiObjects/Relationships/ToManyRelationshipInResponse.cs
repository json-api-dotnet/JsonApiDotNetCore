using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ToManyRelationshipInResponse<TResource>
    where TResource : IIdentifiable
{
    // Non-required because the related controller may be unavailable when used in an include.
    [JsonPropertyName("links")]
    public RelationshipLinks Links { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public ICollection<ResourceIdentifierInResponse<TResource>> Data { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
