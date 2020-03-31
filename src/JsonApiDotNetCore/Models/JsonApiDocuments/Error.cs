using System;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Configuration;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
{
    /// <summary>
    /// Provides additional information about a problem encountered while performing an operation.
    /// Error objects MUST be returned as an array keyed by errors in the top level of a JSON:API document.
    /// </summary>
    public sealed class Error
    {
        public Error() { }

        public Error(HttpStatusCode status, string title)
        {
            Status = status;
            Title = title;
        }

        public Error(HttpStatusCode status, string title, string detail)
        {
            Status = status;
            Title = title;
            Detail = detail;
        }

        /// <summary>
        /// A unique identifier for this particular occurrence of the problem.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// A link that leads to further details about this particular occurrence of the problem.
        /// </summary>
        [JsonProperty("links")]
        public ErrorLinks Links { get; set; }

        public bool ShouldSerializeLinks() => Links?.About != null;

        /// <summary>
        /// The HTTP status code applicable to this problem.
        /// </summary>
        [JsonIgnore]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("status")]
        public string StatusText
        {
            get => Status.ToString("d");
            set => Status = (HttpStatusCode)int.Parse(value);
        }

        /// <summary>
        /// An application-specific error code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// A short, human-readable summary of the problem that SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem. Like title, this field’s value can be localized.
        /// </summary>
        [JsonProperty("detail")]
        public string Detail { get; set; }

        /// <summary>
        /// An object containing references to the source of the error.
        /// </summary>
        [JsonProperty("source")]
        public ErrorSource Source { get; set; }

        public bool ShouldSerializeSource() => Source != null && (Source.Pointer != null || Source.Parameter != null);

        /// <summary>
        /// An object containing non-standard meta-information (key/value pairs) about the error.
        /// </summary>
        [JsonProperty("meta")]
        public ErrorMeta Meta { get; set; }

        public bool ShouldSerializeMeta() => Meta != null && Meta.Data.Any() && !JsonApiOptions.DisableErrorStackTraces;
    }
}
