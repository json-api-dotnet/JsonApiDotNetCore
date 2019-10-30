using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Service to add global top-level metadata to a <see cref="Document"/>.
    /// Use <see cref="IHasMeta"/> on <see cref="ResourceDefinition{TResource}"/>
    /// to specify top-level metadata per resource type.
    /// </summary>
    public interface IRequestMeta
    {
        Dictionary<string, object> GetMeta();
    }
}