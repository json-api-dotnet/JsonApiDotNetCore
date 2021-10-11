#nullable disable

using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-resource-identifier-objects.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceIdentifierObject : IResourceIdentity
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Id { get; set; }

        [JsonPropertyName("lid")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Lid { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
