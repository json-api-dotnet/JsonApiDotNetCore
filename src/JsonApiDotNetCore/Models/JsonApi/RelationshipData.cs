using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models.Links;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Models
{
    public class RelationshipData
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public RelationshipLinks Links { get; set; }

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
                    SingleData = dict;
                else
                    SetManyData(value);
            }
        }

        /// TODO check if behaviour is OK.
        public bool ShouldSerializeExposedData()
        {
            return IsPopulated;
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

        internal bool IsPopulated { get; set; } = false;

        internal bool Any()
        {
            return ((IsHasMany && ManyData.Any()) || SingleData != null);
        }
    }
}
