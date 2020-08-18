using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    public interface IResourceReadRepository<TResource>
       : IResourceReadRepository<TResource, int>
       where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceReadRepository<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Executes a read query using the specified constraints and returns the list of matching resources.
        /// </summary>
        Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer);

        /// <summary>
        /// Executes a read query using the specified top-level filter and returns the top-level count of matching resources.
        /// </summary>
        Task<int> CountAsync(FilterExpression topFilter);
    }
}
