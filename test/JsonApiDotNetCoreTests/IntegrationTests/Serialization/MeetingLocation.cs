using Newtonsoft.Json;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization
{
    public sealed class MeetingLocation
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }
}
