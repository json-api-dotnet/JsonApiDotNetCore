using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoEntityFrameworkExample.Repositories;

/// <summary>
/// Demonstrates how to replace the built-in <see cref="EntityFrameworkCoreRepository{TResource,TId}" />. This read-only repository uses the built-in
/// <see cref="QueryableBuilder" /> to convert the incoming <see cref="QueryLayer" /> into a LINQ expression, then compiles and executes it against the
/// in-memory database.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
public abstract class InMemoryResourceRepository<TResource, TId>(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder, IReadOnlyModel entityModel)
    : IResourceReadRepository<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly ResourceType _resourceType = resourceGraph.GetResourceType<TResource>();
    private readonly QueryLayerToLinqConverter _queryLayerToLinqConverter = new(entityModel, queryableBuilder);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        IEnumerable<TResource> dataSource = GetDataSource();
        IEnumerable<TResource> resources = _queryLayerToLinqConverter.ApplyQueryLayer(queryLayer, dataSource);

        return Task.FromResult<IReadOnlyCollection<TResource>>(resources.ToArray().AsReadOnly());
    }

    /// <inheritdoc />
    public Task<int> CountAsync(FilterExpression? filter, CancellationToken cancellationToken)
    {
        var queryLayer = new QueryLayer(_resourceType)
        {
            Filter = filter
        };

        IEnumerable<TResource> dataSource = GetDataSource();
        IEnumerable<TResource> resources = _queryLayerToLinqConverter.ApplyQueryLayer(queryLayer, dataSource);

        return Task.FromResult(resources.Count());
    }

    protected abstract IEnumerable<TResource> GetDataSource();
}
