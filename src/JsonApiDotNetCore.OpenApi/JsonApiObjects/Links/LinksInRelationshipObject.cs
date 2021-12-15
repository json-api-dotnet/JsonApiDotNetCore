using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class LinksInRelationshipObject
{
    [Required]
    [JsonPropertyName("self")]
    public string Self { get; set; } = null!;

    [Required]
    [JsonPropertyName("related")]
    public string Related { get; set; } = null!;
}
