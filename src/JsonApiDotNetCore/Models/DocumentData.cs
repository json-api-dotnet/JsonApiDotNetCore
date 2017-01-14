using System.Collections.Generic;
using JsonApiDotNetCore.Extensions;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class DocumentData
    {
        private string _type;

        [JsonProperty("type")]
        public string Type
        {
            get { return _type; }
            set { _type = value.Dasherize(); }
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }
    }
}
