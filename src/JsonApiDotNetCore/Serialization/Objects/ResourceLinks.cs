using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class ResourceLinks
    {
        /// <summary>
        /// See https://jsonapi.org/format/#document-resource-object-links.
        /// </summary>
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self);
        }
    }
}
