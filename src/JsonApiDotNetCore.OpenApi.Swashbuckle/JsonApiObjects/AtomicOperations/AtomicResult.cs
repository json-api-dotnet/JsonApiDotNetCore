using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class AtomicResult : IHasMeta
{
    [JsonPropertyName("data")]
    public required ResourceInResponse Data { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
