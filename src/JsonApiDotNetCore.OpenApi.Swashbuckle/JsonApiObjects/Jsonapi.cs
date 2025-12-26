using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class Jsonapi : IHasMeta
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("ext")]
    public required ICollection<string> Ext { get; set; }

    [JsonPropertyName("profile")]
    public required ICollection<string> Profile { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }
}
