using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/ext/atomic/#document-structure.
    /// </summary>
    public sealed class AtomicOperationsDocument
    {
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        [JsonProperty("jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiObject JsonApi { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public TopLevelLinks Links { get; set; }

        [JsonProperty("atomic:operations", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicOperationObject> Operations { get; set; }

        [JsonProperty("atomic:results", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicResultObject> Results { get; set; }
    }
}
