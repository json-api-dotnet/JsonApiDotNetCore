using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class OperationsRequestDocument : IHasMeta
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("atomic:operations")]
    public required ICollection<AtomicOperation> Operations { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
