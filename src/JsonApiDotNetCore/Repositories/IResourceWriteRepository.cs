using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public interface IResourceWriteRepository<TResource>
        : IResourceWriteRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary>
    /// Groups write operations.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface IResourceWriteRepository<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Creates a new resource in the underlying data store.
        /// </summary>
        Task CreateAsync(TResource resource);

        /// <summary>
        /// Adds resources to a to-many relationship in the underlying data store.
        /// </summary>
        Task AddToToManyRelationshipAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds);

        /// <summary>
        /// Updates the attributes and relationships of an existing resource in the underlying data store.
        /// </summary>
        Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase);

        /// <summary>
        /// Performs a complete replacement of the relationship in the underlying data store.
        /// </summary>
        Task SetRelationshipAsync(TResource primaryResource, object secondaryResourceIds);
    
        /// <summary>
        /// Deletes an existing resource from the underlying data store.
        /// </summary>
        Task DeleteAsync(TId id);
        
        /// <summary>
        /// Removes resources from a to-many relationship in the underlying data store.
        /// </summary>
        Task RemoveFromToManyRelationshipAsync(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds);

        /// <summary>
        /// Attempts to retrieve the primary resource during a create/update/delete request.
        /// </summary>
        /// <returns></returns>
        Task<TResource> TryGetPrimaryResourceForUpdateAsync(QueryLayer queryLayer);
    }
}
