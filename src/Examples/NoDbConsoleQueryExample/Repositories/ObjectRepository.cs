using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;

namespace NoDbConsoleQueryExample.Repositories;

/// <summary>
/// A read-only repository that acts on in-memory resources, instead of a database.
/// </summary>
public sealed class ObjectRepository<TResource, TId> : IResourceReadRepository<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IResourceFactory _resourceFactory;
    private readonly IDataSourceProvider<TResource, TId> _dataSourceProvider;

    public ObjectRepository(IResourceGraph resourceGraph, IResourceFactory resourceFactory, IDataSourceProvider<TResource, TId> dataSourceProvider)
    {
        _resourceGraph = resourceGraph;
        _resourceFactory = resourceFactory;
        _dataSourceProvider = dataSourceProvider;
    }

    public Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
    {
        IEnumerable<TResource> query = ApplyQueryLayer(queryLayer);

        IReadOnlyCollection<TResource> resources = query.ToList();
        return Task.FromResult(resources);
    }

    public Task<int> CountAsync(FilterExpression? filter, CancellationToken cancellationToken)
    {
        ResourceType resourceType = _resourceGraph.GetResourceType<TResource>();

        var layer = new QueryLayer(resourceType)
        {
            Filter = filter
        };

        IEnumerable<TResource> resources = ApplyQueryLayer(layer);

        int count = resources.Count();
        return Task.FromResult(count);
    }

    private IEnumerable<TResource> ApplyQueryLayer(QueryLayer queryLayer)
    {
        IQueryable<TResource> source = _dataSourceProvider.Get().AsQueryable();
        Expression expression = GetExpression(source, queryLayer);

        return source.Provider.CreateQuery<TResource>(expression);
    }

    private Expression GetExpression(IQueryable<TResource> source, QueryLayer queryLayer)
    {
        var nameFactory = new LambdaParameterNameFactory();

        var builder = new ObjectQueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory,
            new ResourceModel(_resourceGraph));

        return builder.ApplyQuery(queryLayer);
    }
}
