using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public HttpStatusCode GetErrorStatusCode()
        {
            var statusCodes = Errors
                .Select(e => (int)e.Status)
                .Distinct()
                .ToList();

            if (statusCodes.Count == 1)
                return (HttpStatusCode)statusCodes[0];

            var statusCode = int.Parse(statusCodes.Max().ToString()[0] + "00");
            return (HttpStatusCode)statusCode;
        }

        public IActionResult AsActionResult()
        {
            return new ObjectResult(this)
            {
                StatusCode = (int)GetErrorStatusCode()
            };
        }
    }
}
