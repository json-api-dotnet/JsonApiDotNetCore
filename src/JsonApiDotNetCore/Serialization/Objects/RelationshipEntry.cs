using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class RelationshipEntry : ExposableData<ResourceIdentifierObject>
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public RelationshipLinks Links { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
