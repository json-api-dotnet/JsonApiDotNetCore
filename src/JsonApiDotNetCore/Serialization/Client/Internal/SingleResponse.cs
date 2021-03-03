using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Serialization.Client.Internal
{
    /// <summary>
    /// Represents a deserialized document with "single data".
    /// </summary>
    /// <typeparam name="TResource">
    /// Type of the resource in the primary data.
    /// </typeparam>
    [PublicAPI]
    public sealed class SingleResponse<TResource> : DeserializedResponseBase
        where TResource : class, IIdentifiable
    {
        public TResource Data { get; set; }
    }
}
