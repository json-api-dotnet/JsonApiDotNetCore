using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// https://jsonapi.org/format/1.1/#document-jsonapi-object.
    /// </summary>
    public sealed class JsonApiObject
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Ext { get; set; }

        [JsonProperty("profile", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Profile { get; set; }

        /// <summary>
        /// see "meta" in https://jsonapi.org/format/1.1/#document-meta
        /// </summary>
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
