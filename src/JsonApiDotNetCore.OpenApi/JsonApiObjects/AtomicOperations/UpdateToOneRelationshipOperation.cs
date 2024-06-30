using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class UpdateToOneRelationshipOperation<TResource> : AtomicOperation
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("op")]
    public string Op { get; set; } = null!;

    [Required]
    [JsonPropertyName("ref")]
    public object Ref { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    // Nullability of this property is determined based on the nullability of the to-one relationship.
    public ResourceIdentifierInRequest<TResource>? Data { get; set; }
}
