using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class RelationshipIdentifier<TResource> : IdentifierInRequest<TResource>
    where TResource : IIdentifiable
{
    [Required]
    [JsonPropertyName("relationship")]
    public required string Relationship { get; set; }

    // Meta is erased at runtime.
}
