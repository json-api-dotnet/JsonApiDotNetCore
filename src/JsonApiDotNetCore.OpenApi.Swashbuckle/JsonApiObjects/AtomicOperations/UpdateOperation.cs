using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class UpdateOperation<TResource> : AtomicOperation
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("op")]
    public required string Op { get; set; }

    [JsonPropertyName("ref")]
    public required IdentifierInRequest<TResource> Ref { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public required DataInUpdateRequest<TResource> Data { get; set; }
}
