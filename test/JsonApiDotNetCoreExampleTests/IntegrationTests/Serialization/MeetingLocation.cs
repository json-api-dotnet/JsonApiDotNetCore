using Newtonsoft.Json;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Serialization
{
    public sealed class MeetingLocation
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        
        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }
}
