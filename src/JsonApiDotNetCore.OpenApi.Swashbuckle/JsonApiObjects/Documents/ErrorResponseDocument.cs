using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ErrorResponseDocument : IHasMeta
{
    [JsonPropertyName("jsonapi")]
    public Jsonapi Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public ErrorTopLevelLinks Links { get; set; } = null!;

    [Required]
    [JsonPropertyName("errors")]
    public IList<ErrorObject> Errors { get; set; } = new List<ErrorObject>();

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
