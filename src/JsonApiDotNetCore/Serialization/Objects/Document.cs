#nullable disable

using System.Collections.Generic;
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
    }
}
