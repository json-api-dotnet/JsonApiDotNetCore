using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class CollectionResponseDocument<TResource> : IHasMeta
    where TResource : IIdentifiable
{
    [JsonPropertyName("jsonapi")]
    public required Jsonapi Jsonapi { get; set; }

    [Required]
    [JsonPropertyName("links")]
    public required ResourceCollectionTopLevelLinks Links { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public required ICollection<DataInResponse<TResource>> Data { get; set; }

    [JsonPropertyName("included")]
    public required IList<ResourceInResponse> Included { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
