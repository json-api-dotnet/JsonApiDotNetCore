using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public interface IResourceReadRepository<TResource> : IResourceReadRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Groups read operations.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    [PublicAPI]
    public interface IResourceReadRepository<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Executes a read query using the specified constraints and returns the collection of matching resources.
        /// </summary>
        Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a read query using the specified top-level filter and returns the top-level count of matching resources.
        /// </summary>
        Task<int> CountAsync(FilterExpression topFilter, CancellationToken cancellationToken);
    }
}
