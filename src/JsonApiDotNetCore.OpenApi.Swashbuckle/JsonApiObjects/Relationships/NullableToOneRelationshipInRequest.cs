using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class NullableToOneRelationshipInRequest<TResource>
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("data")]
    public ResourceIdentifierInRequest<TResource>? Data { get; set; }
}
