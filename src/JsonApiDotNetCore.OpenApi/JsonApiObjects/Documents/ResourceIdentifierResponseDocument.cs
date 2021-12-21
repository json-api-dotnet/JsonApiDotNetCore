using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourceIdentifierResponseDocument<TResource> : SingleData<ResourceIdentifierObject<TResource>>
    where TResource : IIdentifiable
{
    [JsonPropertyName("meta")]
    public IDictionary<string, object> Meta { get; set; } = null!;

    [JsonPropertyName("jsonapi")]
    public JsonapiObject Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public LinksInResourceIdentifierDocument Links { get; set; } = null!;
}
