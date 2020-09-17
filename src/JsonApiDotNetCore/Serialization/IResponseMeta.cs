using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Service to add global top-level json:api meta to a response <see cref="Document"/>.
    /// Use <see cref="IResourceDefinition{TResource,TId}.GetMeta"/> to specify top-level metadata per resource type.
    /// </summary>
    public interface IResponseMeta
    {
        IReadOnlyDictionary<string, object> GetMeta();
    }
}
