using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class NullableSecondaryResourceResponseDocument<TResource> : NullableSingleData<ResourceObjectInResponse<TResource>>
    where TResource : IIdentifiable
{
    [JsonPropertyName("meta")]
    public IDictionary<string, object> Meta { get; set; } = null!;

    [JsonPropertyName("jsonapi")]
    public JsonapiObject Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public LinksInResourceDocument Links { get; set; } = null!;
}
