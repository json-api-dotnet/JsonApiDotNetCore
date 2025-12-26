using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class DataInCreateRequest<TResource> : ResourceInCreateRequest
    where TResource : IIdentifiable
{
    [MinLength(1)]
    [JsonPropertyName("id")]
    public override required string Id { get; set; }

    [MinLength(1)]
    [JsonPropertyName("lid")]
    public required string Lid { get; set; }

    [JsonPropertyName("attributes")]
    public required AttributesInCreateRequest<TResource> Attributes { get; set; }

    [JsonPropertyName("relationships")]
    public required RelationshipsInCreateRequest<TResource> Relationships { get; set; }
}
