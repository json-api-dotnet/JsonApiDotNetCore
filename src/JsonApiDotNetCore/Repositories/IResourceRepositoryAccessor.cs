using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Retrieves a <see cref="IResourceRepository{TResource,TId}"/> instance from the D/I container and invokes a callback on it.
    /// </summary>
    public interface IResourceRepositoryAccessor
    {
        /// <summary>
        /// Invokes <see cref="IResourceReadRepository{TResource,TId}.GetAsync"/> for the specified resource type.
        /// </summary>
        Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer);
    }
}
