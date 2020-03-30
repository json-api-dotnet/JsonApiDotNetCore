using System;
using System.Net;
using JsonApiDotNetCore.Configuration;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.JsonApiDocuments
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
    }
}
