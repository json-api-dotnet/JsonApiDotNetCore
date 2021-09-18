using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects
{
    /// <summary>
    /// See https://jsonapi.org/ext/atomic/#result-objects.
    /// </summary>
    [PublicAPI]
    public sealed class AtomicResultObject
    {
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public SingleOrManyData<ResourceObject> Data { get; set; }

        [JsonPropertyName("meta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object> Meta { get; set; }
    }
}
