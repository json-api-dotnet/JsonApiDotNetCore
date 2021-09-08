using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "links" in https://jsonapi.org/format/1.1/#document-resource-object-relationships.
    /// </summary>
    public sealed class RelationshipLinks
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }

        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public string Related { get; set; }

        internal bool HasValue()
        {
            return !string.IsNullOrEmpty(Self) || !string.IsNullOrEmpty(Related);
        }
    }
}
