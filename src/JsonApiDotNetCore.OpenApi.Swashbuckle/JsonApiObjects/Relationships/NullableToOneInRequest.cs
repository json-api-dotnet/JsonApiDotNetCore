using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class NullableToOneInRequest<TResource> : IHasMeta
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("data")]
    public IdentifierInRequest<TResource>? Data { get; set; }

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
