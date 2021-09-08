using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See "links" in https://jsonapi.org/format/1.1/#error-objects.
    /// </summary>
    public sealed class ErrorLinks
    {
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public string About { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}
