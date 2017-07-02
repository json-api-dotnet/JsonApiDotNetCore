using System;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Internal
{
    public class Error
    {
        public Error()
        { }
        
        public Error(string status, string title)
        {
            Status = status;
            Title = title;
        }

        public Error(int status, string title)
        {
            Status = status.ToString();
            Title = title;
        }

        public Error(string status, string title, string detail)
        {
            Status = status;
            Title = title;
            Detail = detail;
        }

        public Error(int status, string title, string detail)
        {
            Status = status.ToString();
            Title = title;
            Detail = detail;
        }
        
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonIgnore]
        public int StatusCode => int.Parse(Status);
    }
}
