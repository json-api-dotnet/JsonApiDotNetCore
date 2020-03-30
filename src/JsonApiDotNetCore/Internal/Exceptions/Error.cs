using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Internal
{
    public class Error
    {
        public Error() { }

        public Error(HttpStatusCode status, string title, ErrorMeta meta = null, ErrorSource source = null)
        {
            Status = status;
            Title = title;
            Meta = meta;
            Source = source;
        }

        public Error(HttpStatusCode status, string title, string detail, ErrorMeta meta = null, ErrorSource source = null)
        {
            Status = status;
            Title = title;
            Detail = detail;
            Meta = meta;
            Source = source;
        }

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("links")]
        public ErrorLinks Links { get; set; }

        [JsonIgnore]
        public HttpStatusCode Status { get; set; }

        [JsonProperty("status")]
        public string StatusText
        {
            get => Status.ToString("d");
            set => Status = (HttpStatusCode)int.Parse(value);
        }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("source")]
        public ErrorSource Source { get; set; }

        [JsonProperty("meta")]
        public ErrorMeta Meta { get; set; }

        public bool ShouldSerializeMeta() => JsonApiOptions.DisableErrorStackTraces == false;
        public bool ShouldSerializeSource() => JsonApiOptions.DisableErrorSource == false;

        public IActionResult AsActionResult()
        {
            var errorCollection = new ErrorCollection
            {
                Errors = new List<Error> { this }
            };

            return errorCollection.AsActionResult();
        }
    }

    public class ErrorLinks
    {
        [JsonProperty("about")]
        public string About { get; set; }
    }

    public class ErrorMeta
    {
        [JsonProperty("stackTrace")]
        public ICollection<string> StackTrace { get; set; }

        public static ErrorMeta FromException(Exception e)
            => new ErrorMeta {
                StackTrace = e.Demystify().ToString().Split(new[] { "\n"}, int.MaxValue, StringSplitOptions.RemoveEmptyEntries)
            };
    }

    public class ErrorSource
    {
        [JsonProperty("pointer")]
        public string Pointer { get; set; }

        [JsonProperty("parameter")]
        public string Parameter { get; set; }
    }
}
