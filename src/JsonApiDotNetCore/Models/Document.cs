using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Document : DocumentBase
    {
        [JsonProperty("data")]
        public DocumentData Data { get; set; }
    }
}
