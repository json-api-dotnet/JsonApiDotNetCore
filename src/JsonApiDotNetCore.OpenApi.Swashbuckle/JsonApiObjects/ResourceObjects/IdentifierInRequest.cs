using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal class IdentifierInRequest : IHasMeta
{
    [Required]
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}

// ReSharper disable once UnusedTypeParameter
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class IdentifierInRequest<TResource> : IdentifierInRequest, IResourceIdentity
    where TResource : IIdentifiable
{
    [MinLength(1)]
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [MinLength(1)]
    [JsonPropertyName("lid")]
    public required string Lid { get; set; }
}
