using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.AtomicOperations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class OperationsRequestDocument
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("atomic:operations")]
    public ICollection<AtomicOperation> Operations { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
