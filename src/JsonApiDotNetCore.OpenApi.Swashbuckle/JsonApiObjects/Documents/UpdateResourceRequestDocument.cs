using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class UpdateResourceRequestDocument<TResource>
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("data")]
    public DataInUpdateResourceRequest<TResource> Data { get; set; } = null!;
}
