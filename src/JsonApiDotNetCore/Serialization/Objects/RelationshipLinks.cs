using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class RelationshipLinks
    {
        /// <summary>
        /// See "links" bulletin at https://jsonapi.org/format/#document-resource-object-relationships.
        /// </summary>
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        /// <summary>
        /// See https://jsonapi.org/format/#document-resource-object-related-resource-links.
        /// </summary>
        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public string Related { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self) || !string.IsNullOrEmpty(Related);
        }
    }
}
