using JsonApiDotNetCore.Models.Links;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Document : DocumentBase
    {
        [JsonProperty("data")]
        public ResourceObject Data { get; set; }
    }
}
