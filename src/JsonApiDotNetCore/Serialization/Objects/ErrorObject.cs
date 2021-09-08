using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#error-objects.
    /// </summary>
    [PublicAPI]
    public sealed class ErrorObject
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorLinks Links { get; set; }

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty("status")]
        public string Status
        {
            get => StatusCode.ToString("d");
            set => StatusCode = (HttpStatusCode)int.Parse(value);
        }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorSource Source { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        public ErrorObject(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
