using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceDataInPatchRequest<TResource> : ResourceData
    where TResource : IIdentifiable
{
    [JsonPropertyName("attributes")]
    public AttributesInPatchRequest<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInPatchRequest<TResource> Relationships { get; set; } = null!;
}
