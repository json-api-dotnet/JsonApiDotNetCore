using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ToManyInResponse<TResource> : IHasMeta
    where TResource : IIdentifiable
{
    // Non-required because the related controller may be unavailable when used in an include.
    [JsonPropertyName("links")]
    public required RelationshipLinks Links { get; set; }

    // Non-required because related data may not be included in the response.
    [JsonPropertyName("data")]
    public required ICollection<IdentifierInResponse<TResource>> Data { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
