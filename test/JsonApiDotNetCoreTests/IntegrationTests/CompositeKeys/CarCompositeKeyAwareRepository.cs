using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class CarCompositeKeyAwareRepository<TResource, TId>(
    IJsonApiRequest request, ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph,
    IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
    IResourceDefinitionAccessor resourceDefinitionAccessor, IQueryableBuilder queryableBuilder)
    : EntityFrameworkCoreRepository<TResource, TId>(request, targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders,
        loggerFactory, resourceDefinitionAccessor, queryableBuilder)
    where TResource : class, IIdentifiable<TId>
{
    private readonly CarExpressionRewriter _writer = new(resourceGraph);

    protected override IQueryable<TResource> ApplyQueryLayer(QueryLayer queryLayer)
    {
        RecursiveRewriteFilterInLayer(queryLayer);

        return base.ApplyQueryLayer(queryLayer);
    }

    private void RecursiveRewriteFilterInLayer(QueryLayer queryLayer)
    {
        if (queryLayer.Filter != null)
        {
            queryLayer.Filter = (FilterExpression?)_writer.Visit(queryLayer.Filter, null);
        }

        if (queryLayer.Sort != null)
        {
            queryLayer.Sort = (SortExpression?)_writer.Visit(queryLayer.Sort, null);
        }

        if (queryLayer.Selection is { IsEmpty: false })
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            foreach (QueryLayer? nextLayer in queryLayer.Selection
                .GetResourceTypes()
                .Select(queryLayer.Selection.GetOrCreateSelectors)
                .SelectMany(selectors => selectors
                    .Select(selector => selector.Value)
                    .Where(layer => layer != null)))
            {
                RecursiveRewriteFilterInLayer(nextLayer!);
            }

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore
        }
    }
}
