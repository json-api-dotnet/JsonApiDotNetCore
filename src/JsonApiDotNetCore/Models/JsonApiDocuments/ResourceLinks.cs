using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.Links
{
    public class ResourceLinks
    {
        /// <summary>
        /// https://jsonapi.org/format/#document-resource-object-links
        /// </summary>
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public string Self { get; set; }
    }
}