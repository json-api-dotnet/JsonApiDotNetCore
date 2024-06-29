using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class DataInCreateResourceRequest<TResource> : ResourceData
    where TResource : IIdentifiable
{
    [MinLength(1)]
    [JsonPropertyName("id")]
    public override string Id { get; set; } = null!;

    [MinLength(1)]
    [JsonPropertyName("lid")]
    public string Lid { get; set; } = null!;

    [JsonPropertyName("attributes")]
    public AttributesInCreateResourceRequest<TResource> Attributes { get; set; } = null!;

    [JsonPropertyName("relationships")]
    public RelationshipsInCreateResourceRequest<TResource> Relationships { get; set; } = null!;
}
