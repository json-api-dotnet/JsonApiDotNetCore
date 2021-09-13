using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/format/1.1/#document-top-level and https://jsonapi.org/ext/atomic/#document-structure.
    /// </summary>
    public sealed class Document
    {
        [JsonPropertyName("jsonapi")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonApiObject JsonApi { get; set; }

        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TopLevelLinks Links { get; set; }

        [JsonPropertyName("data")]
        // JsonIgnoreCondition is determined at runtime by WriteOnlyDocumentConverter.
        public SingleOrManyData<ResourceObject> Data { get; set; }

        [JsonPropertyName("atomic:operations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<AtomicOperationObject> Operations { get; set; }

        [JsonPropertyName("atomic:results")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<AtomicResultObject> Results { get; set; }

        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ErrorObject> Errors { get; set; }

        [JsonPropertyName("included")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ResourceObject> Included { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object> Meta { get; set; }

        // [TODO-STJ]: Inline
        [JsonIgnore]
        public bool IsManyData => Data.ManyValue != null;

        // [TODO-STJ]: Inline
        [JsonIgnore]
        public IList<ResourceObject> ManyData => Data.ManyValue;

        // [TODO-STJ]: Inline
        [JsonIgnore]
        public ResourceObject SingleData => Data.SingleValue;

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
