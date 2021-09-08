using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-resource-object-links.
    /// </summary>
    public sealed class ResourceLinks
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self);
        }
    }
}
