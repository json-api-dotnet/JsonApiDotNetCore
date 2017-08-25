using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonApiDotNetCore.Models.Operations
{
    public class Operation : DocumentBase
    {
        [JsonProperty("op")]
        public OperationCode Op { get; set; }

        [JsonProperty("ref")]
        public ResourceReference Ref { get; set; }

        [JsonProperty("params")]
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
            if (data is JArray jArray) {
                DataIsList = true;
                DataList = jArray.ToObject<List<DocumentData>>();
            }
            else if (data is List<DocumentData> dataList) {
                DataIsList = true;
                DataList = dataList;
            }
            else if (data is JObject jObject) {
                DataObject = jObject.ToObject<DocumentData>();
            }
            else if (data is DocumentData dataObject) {
                DataObject = dataObject;
            }
        }

        public bool DataIsList { get; private set; }
        public List<DocumentData> DataList { get; private set; }
        public DocumentData DataObject { get; private set; }
    }
}
