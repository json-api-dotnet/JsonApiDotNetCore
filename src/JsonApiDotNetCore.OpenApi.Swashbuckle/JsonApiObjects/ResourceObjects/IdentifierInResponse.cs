using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

// ReSharper disable once UnusedTypeParameter
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class IdentifierInResponse<TResource> : IResourceIdentity
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [Required]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
