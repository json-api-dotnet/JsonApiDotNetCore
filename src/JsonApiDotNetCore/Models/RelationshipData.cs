using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Models
{
    public class RelationshipData
    {
        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("data")]
        public object ExposedData { 
            get {
                if(ManyData != null)
                    return ManyData;
                return SingleData;
            }
            set {
                if(value is IEnumerable)
                    if(value is JObject jObject)
                        SingleData = jObject.ToObject<Dictionary<string, string>>();   
                    else if(value is JArray jArray)
                        ManyData = jArray.ToObject<List<Dictionary<string, string>>>();
                    else
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
