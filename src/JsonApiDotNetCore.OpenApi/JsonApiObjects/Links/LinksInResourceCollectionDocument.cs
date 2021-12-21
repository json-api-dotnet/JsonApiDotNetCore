using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class LinksInResourceCollectionDocument
{
    [Required]
    [JsonPropertyName("self")]
    public string Self { get; set; } = null!;

    [JsonPropertyName("describedby")]
    public string Describedby { get; set; } = null!;

    [Required]
    [JsonPropertyName("first")]
    public string First { get; set; } = null!;

    [JsonPropertyName("last")]
    public string Last { get; set; } = null!;

    [JsonPropertyName("prev")]
    public string Prev { get; set; } = null!;

    [JsonPropertyName("next")]
    public string Next { get; set; } = null!;
}
