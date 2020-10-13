using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Retrieves a <see cref="IResourceRepository{TResource,TId}"/> instance from the D/I container and invokes a callback on it.
    /// </summary>
    public interface IResourceRepositoryAccessor
    {
        /// <summary>
        /// Gets resources by filtering on id.
        /// </summary>
        /// <param name="resourceType">The type for which to create a repository.</param>
        /// <param name="ids">The ids to filter on.</param>
        Task<IEnumerable<IIdentifiable>> GetResourcesByIdAsync(Type resourceType, IReadOnlyCollection<string> ids);
    }
}
