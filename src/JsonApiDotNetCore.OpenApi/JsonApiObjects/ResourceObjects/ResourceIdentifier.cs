using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;

// ReSharper disable once UnusedTypeParameter
internal sealed class ResourceIdentifier<TResource> : ResourceIdentifier
    where TResource : IIdentifiable;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class ResourceIdentifier
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
}
