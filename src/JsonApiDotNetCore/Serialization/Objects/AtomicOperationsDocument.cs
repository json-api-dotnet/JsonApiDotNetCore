using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// https://jsonapi.org/ext/atomic/#document-structure
    /// </summary>
    public class AtomicOperationsDocument
    {
        [JsonProperty("operations")]
        public IList<AtomicOperation> Operations { get; set; }
    }
}
