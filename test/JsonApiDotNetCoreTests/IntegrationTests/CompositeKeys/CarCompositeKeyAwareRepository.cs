using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class CarCompositeKeyAwareRepository<TResource, TId>(
    ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : EntityFrameworkCoreRepository<TResource, TId>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor)
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
            foreach (QueryLayer? nextLayer in queryLayer.Selection.GetResourceTypes()
                .Select(resourceType => queryLayer.Selection.GetOrCreateSelectors(resourceType))
                .SelectMany(selectors => selectors.Select(selector => selector.Value).Where(layer => layer != null)))
            {
                RecursiveRewriteFilterInLayer(nextLayer!);
            }
        }
    }
}
