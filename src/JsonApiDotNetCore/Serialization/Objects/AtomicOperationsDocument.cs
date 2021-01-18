using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/ext/atomic/#document-structure.
    /// </summary>
    public sealed class AtomicOperationsDocument
    {
        /// <summary>
        /// See "meta" in https://jsonapi.org/format/#document-top-level.
        /// </summary>
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        /// <summary>
        /// See "jsonapi" in https://jsonapi.org/format/#document-top-level.
        /// </summary>
        [JsonProperty("jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> JsonApi { get; set; }

        /// <summary>
        /// See "links" in https://jsonapi.org/format/#document-top-level.
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public TopLevelLinks Links { get; set; }

        /// <summary>
        /// See https://jsonapi.org/ext/atomic/#operation-objects.
        /// </summary>
        [JsonProperty("atomic:operations", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicOperationObject> Operations { get; set; }

        /// <summary>
        /// See https://jsonapi.org/ext/atomic/#result-objects.
        /// </summary>
        [JsonProperty("atomic:results", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicResultObject> Results { get; set; }
    }
}
