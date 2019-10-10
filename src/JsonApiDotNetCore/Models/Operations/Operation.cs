using System.Collections.Generic;
using JsonApiDotNetCore.Models.Links;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Models.Operations
{
    public class Operation
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public TopLevelLinks Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        public List<ResourceObject> Included { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Meta { get; set; }

        [JsonProperty("op"), JsonConverter(typeof(StringEnumConverter))]
        public OperationCode Op { get; set; }

        [JsonProperty("ref", NullValueHandling = NullValueHandling.Ignore)]
        public ResourceReference Ref { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Params Params { get; set; }

        [JsonProperty("data")]
        public object Data
        {
            get
            {
                if (DataIsList) return DataList;
                return DataObject;
            }
            set => SetData(value);
        }

        private void SetData(object data)
        {
            if (data is JArray jArray)
            {
                DataIsList = true;
                DataList = jArray.ToObject<List<ResourceObject>>();
            }
            else if (data is List<ResourceObject> dataList)
            {
                DataIsList = true;
                DataList = dataList;
            }
            else if (data is JObject jObject)
            {
                DataObject = jObject.ToObject<ResourceObject>();
            }
            else if (data is ResourceObject dataObject)
            {
                DataObject = dataObject;
            }
        }

        [JsonIgnore]
        public bool DataIsList { get; private set; }

        [JsonIgnore]
        public List<ResourceObject> DataList { get; private set; }

        [JsonIgnore]
        public ResourceObject DataObject { get; private set; }

        public string GetResourceTypeName()
        {
            if (Ref != null)
                return Ref.Type?.ToString();

            if (DataIsList)
                return DataList[0].Type?.ToString();

            return DataObject.Type?.ToString();
        }
    }
}
