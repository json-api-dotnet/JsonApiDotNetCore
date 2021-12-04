using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories;

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
    Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a read query using the specified filter and returns the count of matching resources.
    /// </summary>
    Task<int> CountAsync(FilterExpression? filter, CancellationToken cancellationToken);
}
