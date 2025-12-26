using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal abstract class AtomicOperation : IHasMeta
{
    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
