using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/ext/atomic/#operation-objects.
    /// </summary>
    [PublicAPI]
    public sealed class AtomicOperationObject
    {
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public SingleOrManyData<ResourceObject> Data { get; set; }

        // [TODO-STJ]: Inline
        [JsonIgnore]
        public IList<ResourceObject> ManyData => Data.ManyValue;

        // [TODO-STJ]: Inline
        [JsonIgnore]
        public ResourceObject SingleData => Data.SingleValue;

        [JsonPropertyName("op")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public AtomicOperationCode Code { get; set; }

        [JsonPropertyName("ref")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AtomicReference Ref { get; set; }

        [JsonPropertyName("href")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Href { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
