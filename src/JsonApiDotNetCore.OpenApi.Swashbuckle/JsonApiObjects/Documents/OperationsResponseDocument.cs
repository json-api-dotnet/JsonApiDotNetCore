using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Links;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class OperationsResponseDocument : IHasMeta
{
    [JsonPropertyName("jsonapi")]
    public required Jsonapi Jsonapi { get; set; }

    [Required]
    [JsonPropertyName("links")]
    public required ResourceTopLevelLinks Links { get; set; }

    [Required]
    [MinLength(1)]
    [JsonPropertyName("atomic:results")]
    public required IList<AtomicResult> Results { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
