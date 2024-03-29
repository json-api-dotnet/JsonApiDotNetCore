using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourcePostRequestDocument<TResource>
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("data")]
    public ResourceDataInPostRequest<TResource> Data { get; set; } = null!;
}
