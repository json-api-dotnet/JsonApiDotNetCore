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
        /// Updates an existing resource in the underlying data store.
        /// </summary>
        /// <param name="requestResource">The (partial) resource coming from the request body.</param>
        /// <param name="databaseResource">The resource as stored in the database before the update.</param>
        Task UpdateAsync(TResource requestResource, TResource databaseResource);

        /// <summary>
        /// Updates relationships in the underlying data store.
        /// </summary>
        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds);

        /// <summary>
        /// Deletes a resource from the underlying data store.
        /// </summary>
        /// <param name="id">Identifier for the resource to delete.</param>
        /// <returns><c>true</c> if the resource was deleted; <c>false</c> is the resource did not exist.</returns>
        Task<bool> DeleteAsync(TId id);

        /// <summary>
        /// Ensures that the next time this resource is requested, it is re-fetched from the underlying data store.
        /// </summary>
        void FlushFromCache(TResource resource);
    }
}
