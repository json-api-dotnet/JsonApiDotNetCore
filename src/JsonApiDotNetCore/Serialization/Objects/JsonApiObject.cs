using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-jsonapi-object.
    /// </summary>
    public sealed class JsonApiObject
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Ext { get; set; }

        [JsonProperty("profile", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Profile { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
