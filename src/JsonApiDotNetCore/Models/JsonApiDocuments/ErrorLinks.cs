using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    public class ErrorLinks
    {
        [JsonProperty("about")]
        public string About { get; set; }
    }
}
