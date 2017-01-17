using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models
{
    public class RelationshipData
    {
        [JsonProperty("links")]
        public Dictionary<string, string> Links { get; set; }

        [JsonProperty("data")]
        public object ExposedData { 
            get {
                if(ManyData != null)
                    return ManyData;
                return SingleData;
            }
            set {
                if(value is IEnumerable)
                    ManyData = (List<Dictionary<string, string>>)value;
                else
                    SingleData = (Dictionary<string, string>)value;
            }
         }

        [JsonIgnore]
        public List<Dictionary<string, string>> ManyData { get; set; }
        
        [JsonIgnore]
        public Dictionary<string, string> SingleData { get; set; }
    }
}
