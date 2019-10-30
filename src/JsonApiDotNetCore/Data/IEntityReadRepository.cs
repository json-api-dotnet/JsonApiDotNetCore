using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
    public interface IEntityReadRepository<TEntity>
       : IEntityReadRepository<TEntity, int>
       where TEntity : class, IIdentifiable<int>
    { }

    public interface IEntityReadRepository<TEntity, in TId>
        where TEntity : class, IIdentifiable<TId>
    {
        /// <summary>
        /// The base GET query. This is a good place to apply rules that should affect all reads, 
        /// such as authorization of resources.
        /// </summary>
        IQueryable<TEntity> Get();

        /// <summary>
        /// Apply fields to the provided queryable
        /// </summary>
        IQueryable<TEntity> Select(IQueryable<TEntity> entities, List<string> fields);

        /// <summary>
        /// Include a relationship in the query
        /// </summary>
        /// <example>
        /// <code>
        /// _todoItemsRepository.GetAndIncludeAsync(1, "achieved-date");
        /// </code>
        /// </example>
        IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName);

        /// <summary>
        /// Apply a filter to the provided queryable
        /// </summary>
        IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery);

        /// <summary>
        /// Apply a sort to the provided queryable
        /// </summary>
        IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries);

        /// <summary>
        /// Paginate the provided queryable
        /// </summary>
        Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber);

        /// <summary>
        /// Get the entity by id
        /// </summary>
        Task<TEntity> GetAsync(TId id);

        /// <summary>
        /// Get the entity with the specified id and include the relationship.
        /// </summary>
        /// <param name="id">The entity id</param>
        /// <param name="relationshipName">The exposed relationship name</param>
        /// <example>
        /// <code>
        /// _todoItemsRepository.GetAndIncludeAsync(1, "achieved-date");
        /// </code>
        /// </example>
        Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName);

        /// <summary>
        /// Count the total number of records
        /// </summary>
        Task<int> CountAsync(IQueryable<TEntity> entities);

        /// <summary>
        /// Get the first element in the collection, return the default value if collection is empty
        /// </summary>
        Task<TEntity> FirstOrDefaultAsync(IQueryable<TEntity> entities);

        /// <summary>
        /// Convert the collection to a materialized list
        /// </summary>
        Task<IReadOnlyList<TEntity>> ToListAsync(IQueryable<TEntity> entities);
    }
}
