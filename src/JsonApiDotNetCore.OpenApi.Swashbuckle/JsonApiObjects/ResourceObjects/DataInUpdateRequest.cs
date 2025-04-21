using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class DataInUpdateRequest<TResource> : ResourceInUpdateRequest
    where TResource : IIdentifiable
{
    [MinLength(1)]
    [JsonPropertyName("id")]
    public override string Id { get; set; } = null!;

    [MinLength(1)]
    [JsonPropertyName("lid")]
    public string Lid { get; set; } = null!;

    [JsonPropertyName("attributes")]
    public AttributesInUpdateRequest<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInUpdateRequest<TResource> Relationships { get; set; } = null!;
}
