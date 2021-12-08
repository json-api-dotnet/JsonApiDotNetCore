using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// Groups write operations.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
[PublicAPI]
public interface IResourceWriteRepository<TResource, in TId>
    where TResource : class, IIdentifiable<TId>
{
    /// <summary>
    /// Creates a new resource instance, in preparation for <see cref="CreateAsync" />.
    /// </summary>
    /// <remarks>
    /// This method can be overridden to assign resource-specific required relationships.
    /// </remarks>
    Task<TResource> GetForCreateAsync(TId id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new resource in the underlying data store.
    /// </summary>
    Task CreateAsync(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a resource with all of its attributes, including the set of targeted relationships, in preparation for <see cref="UpdateAsync" />.
    /// </summary>
    Task<TResource?> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the attributes and relationships of an existing resource in the underlying data store.
    /// </summary>
    Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an existing resource from the underlying data store.
    /// </summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken);

    /// <summary>
    /// Performs a complete replacement of the relationship in the underlying data store.
    /// </summary>
    Task SetRelationshipAsync(TResource leftResource, object? rightValue, CancellationToken cancellationToken);

    /// <summary>
    /// Adds resources to a to-many relationship in the underlying data store.
    /// </summary>
    Task AddToToManyRelationshipAsync(TId leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken);

    /// <summary>
    /// Removes resources from a to-many relationship in the underlying data store.
    /// </summary>
    Task RemoveFromToManyRelationshipAsync(TResource leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken);
}
