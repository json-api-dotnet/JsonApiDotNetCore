using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Service to add global top-level metadata to a <see cref="Document"/>.
    /// Use <see cref="IResourceDefinition{TResource,TId}.GetMeta"/> to specify top-level metadata per resource type.
    /// </summary>
    public interface IRequestMeta
    {
        IReadOnlyDictionary<string, object> GetMeta();
    }
}
