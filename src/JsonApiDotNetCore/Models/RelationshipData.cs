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
        public object ExposedData
        {
            get
            {
                if (ManyData != null)
                    return ManyData;
                return SingleData;
            }
            set
            {
                if (value is JObject jObject)
                    SingleData = jObject.ToObject<Dictionary<string, object>>();
                else if (value is Dictionary<string, object> dict)
                    SingleData = (Dictionary<string, object>)value;
                else
                    SetManyData(value);
            }
        }

        private void SetManyData(object value)
        {
            IsHasMany = true;
            if (value is JArray jArray)
                ManyData = jArray.ToObject<List<Dictionary<string, object>>>();
            else
                ManyData = (List<Dictionary<string, object>>)value;
        }

        [JsonIgnore]
        public List<Dictionary<string, object>> ManyData { get; set; }

        [JsonIgnore]
        public Dictionary<string, object> SingleData { get; set; }

        [JsonIgnore]
        public bool IsHasMany { get; private set; }
    }
}
