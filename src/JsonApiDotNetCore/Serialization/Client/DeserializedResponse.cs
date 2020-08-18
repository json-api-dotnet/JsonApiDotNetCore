using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Serialization.Client
{
    /// Base class for "single data" and "many data" deserialized responses.
    /// TODO: Currently <see cref="ResourceLinks"/> and <see cref="RelationshipLinks"/>
    /// information is ignored by the serializer. This is out of scope for now because
    /// it is not considered mission critical for v4.
    public abstract class DeserializedResponseBase
    {
        public TopLevelLinks Links { get; set; }
        public IDictionary<string, object> Meta { get; set; }
        public object Errors { get; set; }
        public object JsonApi { get; set; }
    }

    /// <summary>
    /// Represents a deserialized document with "single data".
    /// </summary>
    /// <typeparam name="TResource">Type of the resource in the primary data</typeparam>
    public sealed class SingleResponse<TResource> : DeserializedResponseBase where TResource : class, IIdentifiable
    { 
        public TResource Data { get; set;  }
    }

    /// <summary>
    /// Represents a deserialized document with "many data".
    /// </summary>
    /// <typeparam name="TResource">Type of the resource(s) in the primary data</typeparam>
    public sealed class ManyResponse<TResource> : DeserializedResponseBase where TResource : class, IIdentifiable
    {
        public IReadOnlyCollection<TResource> Data { get; set; }
    }
}
