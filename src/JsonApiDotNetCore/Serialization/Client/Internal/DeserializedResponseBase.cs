using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Client.Internal
{
    /// <summary>
    /// Base class for "single data" and "many data" deserialized responses.
    /// </summary>
    [PublicAPI]
    public abstract class DeserializedResponseBase
    {
        public TopLevelLinks Links { get; set; }
        public IDictionary<string, object> Meta { get; set; }
        public object Errors { get; set; }
        public object JsonApi { get; set; }
    }
}
