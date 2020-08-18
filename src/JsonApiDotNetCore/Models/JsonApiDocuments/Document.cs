using System.Collections.Generic;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    /// <summary>
    /// https://jsonapi.org/format/#document-structure
    /// </summary>
    public sealed class Document : ExposableData<ResourceObject>
    {
        /// <summary>
        /// see "meta" in https://jsonapi.org/format/#document-top-level
        /// </summary>
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        /// <summary>
        /// see "links" in https://jsonapi.org/format/#document-top-level
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public TopLevelLinks Links { get; set; }

        /// <summary>
        /// see "included" in https://jsonapi.org/format/#document-top-level
        /// </summary>
        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public IList<ResourceObject> Included { get; set; }
    }
}
