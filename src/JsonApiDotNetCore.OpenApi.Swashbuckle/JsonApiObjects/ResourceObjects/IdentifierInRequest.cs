using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal class IdentifierInRequest : IHasMeta
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}

// ReSharper disable once UnusedTypeParameter
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class IdentifierInRequest<TResource> : IdentifierInRequest, IResourceIdentity
    where TResource : IIdentifiable
{
    [MinLength(1)]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [MinLength(1)]
    [JsonPropertyName("lid")]
    public string Lid { get; set; } = null!;
}
