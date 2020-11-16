using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    // TODO: @Bart Consider using <TResource>()
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
        Task<int> CountAsync(Type resourceType, FilterExpression topFilter);

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.CreateAsync"/>.
        /// </summary>
        Task CreateAsync<TResource>(TResource resource)
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Invokes <see cref="IResourceWriteRepository{TResource,TId}.AddToToManyRelationshipAsync"/> for the specified resource type.
        /// </summary>
        Task AddToToManyRelationshipAsync<TId>(Type resourceType, TId primaryId, ISet<IIdentifiable> secondaryResourceIds);

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
        Task DeleteAsync<TId>(Type resourceType, TId id);

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
