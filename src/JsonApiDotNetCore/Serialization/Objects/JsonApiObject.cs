using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-jsonapi-object.
    /// </summary>
    [PublicAPI]
    public sealed class JsonApiObject
    {
        [JsonPropertyName("version")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Version { get; set; }

        [JsonPropertyName("ext")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Ext { get; set; }

        [JsonPropertyName("profile")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Profile { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object?>? Meta { get; set; }
    }
}
