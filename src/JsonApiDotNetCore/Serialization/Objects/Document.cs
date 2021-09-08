using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-top-level.
    /// </summary>
    public sealed class Document : ExposableData<ResourceObject>
    {
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        [JsonProperty("jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiObject JsonApi { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public TopLevelLinks Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public IList<ResourceObject> Included { get; set; }
    }
}
