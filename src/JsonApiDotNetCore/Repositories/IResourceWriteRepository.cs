using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public interface IResourceWriteRepository<in TResource>
        : IResourceWriteRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary>
    /// Groups write operations.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface IResourceWriteRepository<in TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Creates a new resource in the underlying data store.
        /// </summary>
        Task CreateAsync(TResource resource);

        /// <summary>
        /// Adds resources to a to-many relationship in the underlying data store.
        /// </summary>
        Task AddToRelationshipAsync(TId id, IReadOnlyCollection<IIdentifiable> secondaryResources);

        /// <summary>
        /// Updates the attributes and relationships of an existing resource in the underlying data store.
        /// </summary>
        Task UpdateAsync(TResource resourceFromRequest, TResource localResource);
        
        /// <summary>
        /// Performs a complete replacement of the value(s) of a relationship in the underlying data store.
        /// </summary>
        Task SetRelationshipAsync(TId id, object secondaryResources);
    
        /// <summary>
        /// Deletes a resource from the underlying data store.
        /// </summary>
        Task DeleteAsync(TId id);
        
        /// <summary>
        /// Removes resources from a to-many relationship in the underlying data store.
        /// </summary>
        Task RemoveFromRelationshipAsync(TId id, IReadOnlyCollection<IIdentifiable> secondaryResources);
        
        /// <summary>
        /// Ensures that the next time a given resource is requested, it is re-fetched from the underlying data store.
        /// </summary>
        void FlushFromCache(TResource resource);
    }
}
