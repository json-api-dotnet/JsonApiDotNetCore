using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

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

        public int GetErrorStatusCode()
        {
            var statusCodes = Errors
                .Select(e => int.Parse(e.Status))
                .Distinct()
                .ToList();

            if (statusCodes.Count == 1)
                return statusCodes[0];

            return int.Parse(statusCodes.Max().ToString()[0] + "00");
        }

        public IActionResult AsActionResult()
        {
            return new ObjectResult(this)
            {
                StatusCode = GetErrorStatusCode()
            };
        }
    }
}
