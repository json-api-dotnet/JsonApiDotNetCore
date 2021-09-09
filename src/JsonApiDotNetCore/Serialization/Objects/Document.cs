using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-top-level and https://jsonapi.org/ext/atomic/#document-structure.
    /// </summary>
    public sealed class Document : ExposableData<ResourceObject>
    {
        [JsonProperty("jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiObject JsonApi { get; set; }

        [JsonProperty("atomic:operations", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicOperationObject> Operations { get; set; }

        [JsonProperty("atomic:results", NullValueHandling = NullValueHandling.Ignore)]
        public IList<AtomicResultObject> Results { get; set; }

        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public IList<ErrorObject> Errors { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Meta { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public TopLevelLinks Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public IList<ResourceObject> Included { get; set; }

        internal HttpStatusCode GetErrorStatusCode()
        {
            if (Errors.IsNullOrEmpty())
            {
                throw new InvalidOperationException("No errors found.");
            }

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
