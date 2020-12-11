using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public class ResourceIdentifierObject
    {
        [JsonProperty("type", Order = -3)]
        public string Type { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore, Order = -2)]
        public string Id { get; set; }

        public override string ToString() => $"(type: {Type}, id: {Id})";
    }
}
