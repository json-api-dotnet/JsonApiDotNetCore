using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class NullableSecondaryResourceResponseDocument<TResource>
    where TResource : IIdentifiable
{
    [JsonPropertyName("jsonapi")]
    public Jsonapi Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public ResourceTopLevelLinks Links { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public ResourceDataInResponse<TResource>? Data { get; set; }

    [JsonPropertyName("included")]
    public IList<ResourceData> Included { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
