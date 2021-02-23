using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Provides a method to obtain global JSON:API meta, which is added at top-level to a response <see cref="Document" />. Use
    /// <see cref="IResourceDefinition{TResource,TId}.GetMeta" /> to specify nested metadata per individual resource.
    /// </summary>
    public interface IResponseMeta
    {
        /// <summary>
        /// Gets the global top-level JSON:API meta information to add to the response.
        /// </summary>
        IReadOnlyDictionary<string, object> GetMeta();
    }
}
