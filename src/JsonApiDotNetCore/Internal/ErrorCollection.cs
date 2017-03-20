using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Internal
{
    public class ErrorCollection
    {
        public ErrorCollection()
        { 
            Errors = new List<Error>();
        }
        
        public List<Error> Errors { get; set; }

        public void Add(Error error)
        {
            Errors.Add(error);
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
