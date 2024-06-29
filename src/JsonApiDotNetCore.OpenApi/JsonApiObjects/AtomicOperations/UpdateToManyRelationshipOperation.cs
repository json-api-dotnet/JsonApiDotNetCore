using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class UpdateToManyRelationshipOperation<TResource> : AtomicOperation
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
    public ICollection<ResourceIdentifierInRequest<TResource>> Data { get; set; } = null!;
}
