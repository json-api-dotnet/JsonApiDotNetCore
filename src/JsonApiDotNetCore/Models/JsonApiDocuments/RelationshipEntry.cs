using JsonApiDotNetCore.Models.Links;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public sealed class RelationshipEntry : ExposableData<ResourceIdentifierObject>
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public RelationshipLinks Links { get; set; }
    }
}
