using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
    public interface IResourceReadRepository<TResource>
       : IResourceReadRepository<TResource, int>
       where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceReadRepository<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// The base GET query. This is a good place to apply rules that should affect all reads, 
        /// such as authorization of resources.
        /// </summary>
        IQueryable<TResource> Get();
        /// <summary>
        /// Get the entity by id
        /// </summary>
        IQueryable<TResource> Get(TId id);
        /// <summary>
        /// Apply fields to the provided queryable
        /// </summary>
        IQueryable<TResource> Select(IQueryable<TResource> entities, IEnumerable<AttrAttribute> fields);
        /// <summary>
        /// Include a relationship in the query
        /// </summary>
        /// <example>
        /// <code>
        /// _todoItemsRepository.GetAndIncludeAsync(1, "achieved-date");
        /// </code>
        /// </example>
        IQueryable<TResource> Include(IQueryable<TResource> entities, IEnumerable<RelationshipAttribute> inclusionChain);
        /// <summary>
        /// Apply a filter to the provided queryable
        /// </summary>
        IQueryable<TResource> Filter(IQueryable<TResource> entities, FilterQueryContext filterQuery);
        /// <summary>
        /// Apply a sort to the provided queryable
        /// </summary>
        IQueryable<TResource> Sort(IQueryable<TResource> entities, SortQueryContext sortQueries);
        /// <summary>
        /// Paginate the provided queryable
        /// </summary>
        Task<IEnumerable<TResource>> PageAsync(IQueryable<TResource> entities, int pageSize, int pageNumber);
        /// <summary>
        /// Count the total number of records
        /// </summary>
        Task<int> CountAsync(IQueryable<TResource> entities);
        /// <summary>
        /// Get the first element in the collection, return the default value if collection is empty
        /// </summary>
        Task<TResource> FirstOrDefaultAsync(IQueryable<TResource> entities);
        /// <summary>
        /// Convert the collection to a materialized list
        /// </summary>
        Task<IReadOnlyList<TResource>> ToListAsync(IQueryable<TResource> entities);
    }
}
