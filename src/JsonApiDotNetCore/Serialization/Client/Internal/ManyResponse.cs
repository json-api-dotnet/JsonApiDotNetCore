using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Serialization.Client.Internal
{
    /// <summary>
    /// Represents a deserialized document with "many data".
    /// </summary>
    /// <typeparam name="TResource">
    /// Type of the resource(s) in the primary data.
    /// </typeparam>
    public sealed class ManyResponse<TResource> : DeserializedResponseBase
        where TResource : class, IIdentifiable
    {
        public IReadOnlyCollection<TResource> Data { get; set; }
    }
}
