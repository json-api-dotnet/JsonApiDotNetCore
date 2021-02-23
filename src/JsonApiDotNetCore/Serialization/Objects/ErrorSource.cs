using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class ErrorSource
    {
        /// <summary>
        /// Optional. A JSON Pointer [RFC6901] to the associated resource in the request document [e.g. "/data" for a primary data object, or
        /// "/data/attributes/title" for a specific attribute].
        /// </summary>
        [JsonProperty]
        public string Pointer { get; set; }

        /// <summary>
        /// Optional. A string indicating which URI query parameter caused the error.
        /// </summary>
        [JsonProperty]
        public string Parameter { get; set; }
    }
}
