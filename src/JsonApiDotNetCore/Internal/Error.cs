using Newtonsoft.Json;

namespace JsonApiDotNetCore.Internal
{
    public class Error
    {
        public Error(string status, string title)
        {
            Status = status;
            Title = title;
        }

        public Error(string status, string title, string detail)
        {
            Status = status;
            Title = title;
            Detail = detail;
        }
        
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
