using System;
using System.Collections.Generic;
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
        Task<IReadOnlyCollection<TResource>> GetAsync<TResource>(QueryLayer layer)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceReadRepository{TResource,TId}.GetAsync"/> for the specified resource type.
        /// </summary>
        Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer);

        /// <summary>
        /// Invokes <see cref="IResourceReadRepository{TResource,TId}.CountAsync"/> for the specified resource type.
        /// </summary>
        Task<int> CountAsync<TResource>(FilterExpression topFilter)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.CreateAsync"/>.
        /// </summary>
        Task CreateAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.AddToToManyRelationshipAsync"/> for the specified resource type.
        /// </summary>
        Task AddToToManyRelationshipAsync<TResource, TId>(TId primaryId, ISet<IIdentifiable> secondaryResourceIds)
            where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.UpdateAsync"/>.
        /// </summary>
        Task UpdateAsync<TResource>(TResource resourceFromRequest, TResource resourceFromDatabase)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.SetRelationshipAsync"/>.
        /// </summary>
        Task SetRelationshipAsync<TResource>(TResource primaryResource, object secondaryResourceIds)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.DeleteAsync"/> for the specified resource type.
        /// </summary>
        Task DeleteAsync<TResource, TId>(TId id)
            where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.RemoveFromToManyRelationshipAsync"/>.
        /// </summary>
        Task RemoveFromToManyRelationshipAsync<TResource>(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.GetForUpdateAsync"/>.
        /// </summary>
        Task<TResource> GetForUpdateAsync<TResource>(QueryLayer queryLayer)
            where TResource : class, IIdentifiable;
    }
}
