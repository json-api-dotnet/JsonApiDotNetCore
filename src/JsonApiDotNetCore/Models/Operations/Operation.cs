using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Models.Operations
{
    public class Operation : DocumentBase
    {
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
                DataList = jArray.ToObject<List<DocumentData>>();
            }
            else if (data is List<DocumentData> dataList)
            {
                DataIsList = true;
                DataList = dataList;
            }
            else if (data is JObject jObject)
            {
                DataObject = jObject.ToObject<DocumentData>();
            }
            else if (data is DocumentData dataObject)
            {
                DataObject = dataObject;
            }
        }

        [JsonIgnore]
        public bool DataIsList { get; private set; }

        [JsonIgnore]
        public List<DocumentData> DataList { get; private set; }

        [JsonIgnore]
        public DocumentData DataObject { get; private set; }

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
