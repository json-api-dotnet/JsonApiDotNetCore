using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

// Types in the JsonApiObjects namespace are never touched by ASP.NET ModelState validation, therefore using a non-nullable reference type for a property does not
// imply this property is required. Instead, we use [Required] explicitly, because this is how Swashbuckle is instructed to mark properties as required.
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class NullableResourceIdentifierResponseDocument<TResource>
    where TResource : IIdentifiable
{
    [JsonPropertyName("jsonapi")]
    public Jsonapi Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public ResourceIdentifierTopLevelLinks Links { get; set; } = null!;

    [Required]
    [JsonPropertyName("data")]
    public ResourceIdentifier<TResource>? Data { get; set; }

    [JsonPropertyName("meta")]
    public IDictionary<string, object> Meta { get; set; } = null!;
}
