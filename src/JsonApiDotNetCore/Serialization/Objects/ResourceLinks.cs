using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-resource-object-links.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceLinks
    {
        [JsonPropertyName("self")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Self { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self);
        }
    }
}
