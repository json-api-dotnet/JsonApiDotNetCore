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
                    SingleData = jObject.ToObject<ResourceIdentifierObject>();
                else if (value is ResourceIdentifierObject dict)
                    SingleData = (ResourceIdentifierObject)value;
                else
                    SetManyData(value);
            }
        }

        private void SetManyData(object value)
        {
            IsHasMany = true;
            if (value is JArray jArray)
                ManyData = jArray.ToObject<List<ResourceIdentifierObject>>();
            else
                ManyData = (List<ResourceIdentifierObject>)value;
        }

        [JsonIgnore]
        public List<ResourceIdentifierObject> ManyData { get; set; }

        [JsonIgnore]
        public ResourceIdentifierObject SingleData { get; set; }

        [JsonIgnore]
        public bool IsHasMany { get; private set; }
    }
}
