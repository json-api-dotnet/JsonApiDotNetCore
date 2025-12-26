using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class UpdateToOneRelationshipOperation<TResource> : AtomicOperation
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
    // Nullability of this property is determined based on the nullability of the to-one relationship.
    public IdentifierInRequest<TResource>? Data { get; set; }
}
