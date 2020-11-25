using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// https://jsonapi.org/ext/atomic/#document-structure
    /// </summary>
    public sealed class AtomicOperationsDocument
    {
        [JsonProperty("atomic:operations", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicOperationObject> Operations { get; set; }

        [JsonProperty("atomic:results", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicResultObject> Results { get; set; }
    }
}
