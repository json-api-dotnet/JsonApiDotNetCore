using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Serialization
{
    /// TODO: Currently <see cref="ResourceLinks"/> and <see cref="RelationshipLinks"/>
    /// information is ignored by the serializer. This is considered not mission critical for now, and therefore out of scope.
    public class DeserializedResponseBase
    {
        public TopLevelLinks Links { get; internal set; }
        public Dictionary<string, object> Meta { get; internal set; }
        public object Errors { get; internal set; }
        public object JsonApi { get; internal set; }
    }

    public class DeserializedSingleResponse<TResource> : DeserializedResponseBase where TResource : class, IIdentifiable
    {
        public TResource Data { get; internal set; }
    }

    public class DeserializedListResponse<TResource> : DeserializedResponseBase where TResource : class, IIdentifiable
    {
        public List<TResource> Data { get; internal set; }
    }
}
