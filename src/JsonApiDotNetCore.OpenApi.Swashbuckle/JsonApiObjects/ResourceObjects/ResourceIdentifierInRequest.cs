using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

// ReSharper disable once UnusedTypeParameter
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class ResourceIdentifierInRequest<TResource> : IResourceIdentity
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [MinLength(1)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [MinLength(1)]
    [JsonPropertyName("lid")]
    public string Lid { get; set; } = null!;
}
