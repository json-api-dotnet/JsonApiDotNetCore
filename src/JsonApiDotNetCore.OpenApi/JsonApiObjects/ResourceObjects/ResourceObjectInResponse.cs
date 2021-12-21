using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceObjectInResponse<TResource> : ResourceIdentifierObject
    where TResource : IIdentifiable
{
    [JsonPropertyName("attributes")]
    public AttributesInResponse<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInResponse<TResource> Relationships { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public LinksInResourceObject Links { get; set; } = null!;

    [JsonPropertyName("meta")]
    public IDictionary<string, object> Meta { get; set; } = null!;
}
