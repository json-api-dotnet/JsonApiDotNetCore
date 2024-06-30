using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects.AtomicOperations;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal abstract class AtomicOperation
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
