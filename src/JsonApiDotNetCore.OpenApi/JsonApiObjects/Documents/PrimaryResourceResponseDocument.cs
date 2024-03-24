using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class PrimaryResourceResponseDocument<TResource> : SingleData<ResourceDataInResponse<TResource>>
    where TResource : IIdentifiable
{
    [JsonPropertyName("jsonapi")]
    public Jsonapi Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public ResourceTopLevelLinks Links { get; set; } = null!;

    [JsonPropertyName("included")]
    public IList<ResourceData> Included { get; set; } = null!;

    [JsonPropertyName("meta")]
    public IDictionary<string, object> Meta { get; set; } = null!;
}
