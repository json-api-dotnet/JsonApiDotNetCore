using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    public sealed class ErrorLinks
    {
        [JsonProperty("about")]
        public string About { get; set; }
    }
}
