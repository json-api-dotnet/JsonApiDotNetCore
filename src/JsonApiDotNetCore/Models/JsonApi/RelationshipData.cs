using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models.Links;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Models
{
    public class RelationshipData : ExposableData<ResourceIdentifierObject>
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public RelationshipLinks Links { get; set; }
    }
}
