using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    public sealed class ErrorSource
    {
        /// <summary>
        /// Optional. A JSON Pointer [RFC6901] to the associated entity in the request document [e.g. "/data" for a primary data object, or "/data/attributes/title" for a specific attribute].
        /// </summary>
        [JsonProperty("pointer")]
        public string Pointer { get; set; }

        /// <summary>
        /// Optional. A string indicating which URI query parameter caused the error.
        /// </summary>
        [JsonProperty("parameter")]
        public string Parameter { get; set; }
    }
}
