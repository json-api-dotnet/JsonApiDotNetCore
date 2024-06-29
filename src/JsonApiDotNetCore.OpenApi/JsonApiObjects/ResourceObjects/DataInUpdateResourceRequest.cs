using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class DataInUpdateResourceRequest<TResource> : ResourceData
    where TResource : IIdentifiable
{
    [JsonPropertyName("attributes")]
    public AttributesInUpdateResourceRequest<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInUpdateResourceRequest<TResource> Relationships { get; set; } = null!;
}
