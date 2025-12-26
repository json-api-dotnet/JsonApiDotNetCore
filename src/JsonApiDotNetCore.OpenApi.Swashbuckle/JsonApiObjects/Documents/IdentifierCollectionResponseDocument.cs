using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class IdentifierCollectionResponseDocument<TResource> : IHasMeta
    where TResource : IIdentifiable
{
    [JsonPropertyName("jsonapi")]
    public required Jsonapi Jsonapi { get; set; }

    [Required]
    [JsonPropertyName("links")]
    public required ResourceIdentifierCollectionTopLevelLinks Links { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public required ICollection<IdentifierInResponse<TResource>> Data { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
