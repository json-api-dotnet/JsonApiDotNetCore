using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class CreateResourceOperation<TResource> : AtomicOperation
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("op")]
    public string Op { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public DataInCreateResourceRequest<TResource> Data { get; set; } = null!;
}
