using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class OperationsResponseDocument
{
    [JsonPropertyName("jsonapi")]
    public Jsonapi Jsonapi { get; set; } = null!;

    [Required]
    [JsonPropertyName("links")]
    public ResourceTopLevelLinks Links { get; set; } = null!;

    [Required]
    [MinLength(1)]
    [JsonPropertyName("atomic:results")]
    public IList<AtomicResult> Results { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
