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
    public required Jsonapi Jsonapi { get; set; }

    [Required]
    [JsonPropertyName("links")]
    public required ErrorTopLevelLinks Links { get; set; }

    [Required]
    [JsonPropertyName("errors")]
    public IList<ErrorObject> Errors { get; set; } = new List<ErrorObject>();

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
