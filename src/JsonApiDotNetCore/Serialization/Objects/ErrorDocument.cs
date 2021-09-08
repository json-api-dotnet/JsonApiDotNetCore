using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-top-level.
    /// </summary>
    public sealed class ErrorDocument
    {
        [JsonProperty("errors")]
        public IList<ErrorObject> Errors { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        internal HttpStatusCode GetErrorStatusCode()
        {
            int[] statusCodes = Errors.Select(error => (int)error.StatusCode).Distinct().ToArray();

            if (statusCodes.Length == 1)
            {
                return (HttpStatusCode)statusCodes[0];
            }

            int statusCode = int.Parse($"{statusCodes.Max().ToString()[0]}00");
            return (HttpStatusCode)statusCode;
        }
    }
}
