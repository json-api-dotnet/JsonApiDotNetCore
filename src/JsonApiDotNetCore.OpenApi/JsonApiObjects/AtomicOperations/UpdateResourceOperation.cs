using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class UpdateResourceOperation<TResource> : AtomicOperation
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("op")]
    public string Op { get; set; } = null!;

    [JsonPropertyName("ref")]
    public ResourceIdentifierInRequest<TResource> Ref { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public DataInUpdateResourceRequest<TResource> Data { get; set; } = null!;
}
