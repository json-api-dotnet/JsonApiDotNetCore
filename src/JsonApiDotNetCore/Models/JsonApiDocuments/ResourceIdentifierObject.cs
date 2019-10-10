using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class ResourceIdentifierObject
    {
        public ResourceIdentifierObject() { }
        public ResourceIdentifierObject(string type, string id)
        {
            Type = type;
            Id = id;
        }

        [JsonProperty("type", Order = -3)]
        public string Type { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore, Order = -2)]
        public string Id { get; set; }

        public override string ToString() => $"(type: {Type}, id: {Id})";
    }
}
