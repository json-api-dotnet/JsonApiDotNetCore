using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Retrieves a <see cref="IResourceRepository{TResource,TId}"/> instance from the D/I container and invokes a callback on it.
    /// </summary>
    public interface IResourceRepositoryAccessor
    {
        /// <summary>
        /// Invokes <see cref="IResourceReadRepository{TResource,TId}.GetAsync"/>.
        /// </summary>
        Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer layer, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceReadRepository{TResource,TId}.GetAsync"/> for the specified resource type.
        /// </summary>
        Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes <see cref="IResourceReadRepository{TResource,TId}.CountAsync"/> for the specified resource type.
        /// </summary>
        Task<int> CountAsync<TResource>(FilterExpression topFilter, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.GetForCreateAsync"/>.
        /// </summary>
        Task<TResource> GetForCreateAsync<TResource, TId>(TId id, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.CreateAsync"/>.
        /// </summary>
        Task CreateAsync<TResource>(TResource resourceFromRequest, TResource resourceForDatabase, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.GetForUpdateAsync"/>.
        /// </summary>
        Task<TResource> GetForUpdateAsync<TResource>(QueryLayer queryLayer, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.UpdateAsync"/>.
        /// </summary>
        Task UpdateAsync<TResource>(TResource resourceFromRequest, TResource resourceFromDatabase, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.DeleteAsync"/> for the specified resource type.
        /// </summary>
        Task DeleteAsync<TResource, TId>(TId id, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.SetRelationshipAsync"/>.
        /// </summary>
        Task SetRelationshipAsync<TResource>(TResource primaryResource, object secondaryResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.AddToToManyRelationshipAsync"/> for the specified resource type.
        /// </summary>
        Task AddToToManyRelationshipAsync<TResource, TId>(TId primaryId, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.RemoveFromToManyRelationshipAsync"/>.
        /// </summary>
        Task RemoveFromToManyRelationshipAsync<TResource>(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
            where TResource : class, IIdentifiable;
    }
}
