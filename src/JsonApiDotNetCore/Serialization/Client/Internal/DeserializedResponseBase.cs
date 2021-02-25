using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Client.Internal
{
    /// Base class for "single data" and "many data" deserialized responses.
    /// TODO: Currently
    /// <see cref="ResourceLinks" />
    /// and
    /// <see cref="RelationshipLinks" />
    /// information is ignored by the serializer. This is out of scope for now because
    /// it is not considered mission critical for v4.
    [PublicAPI]
    public abstract class DeserializedResponseBase
    {
        public TopLevelLinks Links { get; set; }
        public IDictionary<string, object> Meta { get; set; }
        public object Errors { get; set; }
        public object JsonApi { get; set; }
    }
}
