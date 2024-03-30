using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ToManyRelationshipInRequest<TResource>
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("data")]
    public ICollection<ResourceIdentifier<TResource>> Data { get; set; } = null!;
}
