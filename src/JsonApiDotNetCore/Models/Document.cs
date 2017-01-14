using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class Document
    {
        [JsonProperty("data")]
        public DocumentData Data { get; set; }
    }
}
