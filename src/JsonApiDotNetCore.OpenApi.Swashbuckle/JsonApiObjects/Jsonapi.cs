using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class Jsonapi : IHasMeta
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = null!;

    [JsonPropertyName("ext")]
    public ICollection<string> Ext { get; set; } = null!;

    [JsonPropertyName("profile")]
    public ICollection<string> Profile { get; set; } = null!;

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
}
