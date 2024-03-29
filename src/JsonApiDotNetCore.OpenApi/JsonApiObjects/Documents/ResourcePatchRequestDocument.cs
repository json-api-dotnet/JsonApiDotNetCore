using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ResourcePatchRequestDocument<TResource>
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("data")]
    public ResourceDataInPatchRequest<TResource> Data { get; set; } = null!;
}
