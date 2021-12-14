using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MeetingLocation
{
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lng")]
    public double Longitude { get; set; }
}
