using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.Links
{
    public sealed class RelationshipLinks
    {
        /// <summary>
        /// see "links" bulletin at https://jsonapi.org/format/#document-resource-object-relationships
        /// </summary>
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        /// <summary>
        /// https://jsonapi.org/format/#document-resource-object-related-resource-links
        /// </summary>
        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public string Related { get; set; }
    }
}
