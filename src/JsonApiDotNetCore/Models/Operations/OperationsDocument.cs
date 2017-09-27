using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Models.Operations
{
    public class OperationsDocument
    {
        public OperationsDocument() { }
        public OperationsDocument(List<Operation> operations) 
        { 
            Operations = operations;
        }
        
        [JsonProperty("operations")]
        public List<Operation> Operations { get; set; }
    }
}
