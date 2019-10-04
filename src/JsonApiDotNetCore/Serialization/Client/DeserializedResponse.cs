using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Serialization.Client
{
    /// Base class for "single data" and "many data" deserialized responses.
    /// TODO: Currently <see cref="ResourceLinks"/> and <see cref="RelationshipLinks"/>
    /// information is ignored by the serializer. This is out of scope for now because
    /// it is not considered mission critical for v4.
    public class DeserializedResponseBase
    {
        public TopLevelLinks Links { get; internal set; }
        public Dictionary<string, object> Meta { get; internal set; }
        public object Errors { get; internal set; }
        public object JsonApi { get; internal set; }
    }

    /// <summary>
    /// Represents a deserialized document with "single data".
    /// </summary>
    /// <typeparam name="TResource">Type of the resource in the primary data</typeparam>
    public class DeserializedSingleResponse<TResource> : DeserializedResponseBase where TResource : class, IIdentifiable
    {
        public TResource Data { get; internal set; }
    }

    /// <summary>
    /// Represents a deserialized document with "many data".
    /// </summary>
    /// <typeparam name="TResource">Type of the resource(s) in the primary data</typeparam>
    public class DeserializedListResponse<TResource> : DeserializedResponseBase where TResource : class, IIdentifiable
    {
        public List<TResource> Data { get; internal set; }
    }
}
