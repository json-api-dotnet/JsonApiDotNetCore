using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class RemoveFromRelationshipOperation<TResource> : AtomicOperation
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("op")]
    public required string Op { get; set; }

    [Required]
    [JsonPropertyName("ref")]
    public required object Ref { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public required ICollection<IdentifierInRequest<TResource>> Data { get; set; }
}
