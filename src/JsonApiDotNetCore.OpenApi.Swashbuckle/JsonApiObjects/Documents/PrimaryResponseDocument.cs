using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class PrimaryResponseDocument<TResource> : IHasMeta
    where TResource : IIdentifiable
{
    [JsonPropertyName("jsonapi")]
    public Jsonapi Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public ResourceTopLevelLinks Links { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public DataInResponse<TResource> Data { get; set; } = null!;

    [JsonPropertyName("included")]
    public IList<ResourceInResponse> Included { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
