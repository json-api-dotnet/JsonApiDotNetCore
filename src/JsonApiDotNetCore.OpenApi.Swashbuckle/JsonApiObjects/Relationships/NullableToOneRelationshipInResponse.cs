using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class NullableToOneRelationshipInResponse<TResource>
    where TResource : IIdentifiable
{
    // Non-required because the related controller may be unavailable when used in an include.
    [JsonPropertyName("links")]
    public RelationshipLinks Links { get; set; } = null!;

    // Non-required because related data may not be included in the response.
    [JsonPropertyName("data")]
    public ResourceIdentifierInResponse<TResource>? Data { get; set; }

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
