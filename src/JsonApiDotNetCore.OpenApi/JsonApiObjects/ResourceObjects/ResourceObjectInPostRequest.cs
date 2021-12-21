using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceObjectInPostRequest<TResource> : ResourceIdentifierObject
    where TResource : IIdentifiable
{
    [JsonPropertyName("attributes")]
    public AttributesInPostRequest<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInPostRequest<TResource> Relationships { get; set; } = null!;
}
