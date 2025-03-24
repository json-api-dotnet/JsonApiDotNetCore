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
    public string Op { get; set; } = null!;

    [Required]
    [JsonPropertyName("ref")]
    public object Ref { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public ICollection<IdentifierInRequest<TResource>> Data { get; set; } = null!;
}
