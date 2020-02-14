using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Serialization.Common;
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

        public string GetJson(JsonSerializerSettings serializerSettings)
        {
            var beforeContractResolver = serializerSettings.ContractResolver;
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            using var scope = new JsonSerializerSettingsNullValueHandlingScope(serializerSettings, NullValueHandling.Ignore);
            string json = JsonConvert.SerializeObject(this, serializerSettings);

            serializerSettings.ContractResolver = beforeContractResolver;

            return json;
        }

        public int GetErrorStatusCode()
        {
            var statusCodes = Errors
                .Select(e => e.StatusCode)
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
