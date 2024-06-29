using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceDataInResponse<TResource> : ResourceData
    where TResource : IIdentifiable
{
    [JsonPropertyName("attributes")]
    public AttributesInResponse<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInResponse<TResource> Relationships { get; set; } = null!;

    // Non-required because the related controller may be unavailable when used in an include.
    [JsonPropertyName("links")]
    public ResourceLinks Links { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
