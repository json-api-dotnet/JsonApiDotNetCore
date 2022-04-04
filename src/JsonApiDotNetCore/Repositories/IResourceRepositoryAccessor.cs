using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories;

/// <summary>
/// Retrieves an <see cref="IResourceRepository{TResource,TId}" /> instance from the D/I container and invokes a method on it.
/// </summary>
public interface IResourceRepositoryAccessor
{
    /// <summary>
    /// Invokes <see cref="IResourceReadRepository{TResource,TId}.GetAsync" /> for the specified resource type.
    /// </summary>
    Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer queryLayer, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Invokes <see cref="IResourceReadRepository{TResource,TId}.GetAsync" /> for the specified resource type.
    /// </summary>
    Task<IReadOnlyCollection<IIdentifiable>> GetAsync(ResourceType resourceType, QueryLayer queryLayer, CancellationToken cancellationToken);

    /// <summary>
    /// Invokes <see cref="IResourceReadRepository{TResource,TId}.CountAsync" /> for the specified resource type.
    /// </summary>
    Task<int> CountAsync(ResourceType resourceType, FilterExpression? filter, CancellationToken cancellationToken);

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.GetForCreateAsync" /> for the specified resource type.
    /// </summary>
    Task<TResource> GetForCreateAsync<TResource, TId>(Type resourceClrType, TId id, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable<TId>;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.CreateAsync" /> for the specified resource type.
    /// </summary>
    Task CreateAsync<TResource>(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.GetForUpdateAsync" /> for the specified resource type.
    /// </summary>
    Task<TResource?> GetForUpdateAsync<TResource>(QueryLayer queryLayer, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.UpdateAsync" /> for the specified resource type.
    /// </summary>
    Task UpdateAsync<TResource>(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.DeleteAsync" /> for the specified resource type.
    /// </summary>
    Task DeleteAsync<TResource, TId>(TResource? resourceFromDatabase, TId id, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable<TId>;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.SetRelationshipAsync" /> for the specified resource type.
    /// </summary>
    Task SetRelationshipAsync<TResource>(TResource leftResource, object? rightValue, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.AddToToManyRelationshipAsync" /> for the specified resource type.
    /// </summary>
    Task AddToToManyRelationshipAsync<TResource, TId>(TResource? leftResource, TId leftId, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
        where TResource : class, IIdentifiable<TId>;

    /// <summary>
    /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.RemoveFromToManyRelationshipAsync" /> for the specified resource type.
    /// </summary>
    Task RemoveFromToManyRelationshipAsync<TResource>(TResource leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        where TResource : class, IIdentifiable;
}
