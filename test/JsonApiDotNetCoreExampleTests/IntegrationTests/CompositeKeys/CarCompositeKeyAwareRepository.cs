using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class CarCompositeKeyAwareRepository<TResource, TId> : EntityFrameworkCoreRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly CarExpressionRewriter _writer;

        public CarCompositeKeyAwareRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
            IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor)
        {
            _writer = new CarExpressionRewriter(resourceGraph);
        }

        protected override IQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            RecursiveRewriteFilterInLayer(layer);

            return base.ApplyQueryLayer(layer);
        }

        private void RecursiveRewriteFilterInLayer(QueryLayer queryLayer)
        {
            if (queryLayer.Filter != null)
            {
                queryLayer.Filter = (FilterExpression)_writer.Visit(queryLayer.Filter, null);
            }

            if (queryLayer.Sort != null)
            {
                queryLayer.Sort = (SortExpression)_writer.Visit(queryLayer.Sort, null);
            }

            if (queryLayer.Projection != null)
            {
                foreach (QueryLayer nextLayer in queryLayer.Projection.Values.Where(layer => layer != null))
                {
                    RecursiveRewriteFilterInLayer(nextLayer);
                }
            }
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class CarCompositeKeyAwareRepository<TResource> : CarCompositeKeyAwareRepository<TResource, int>, IResourceRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public CarCompositeKeyAwareRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph,
            IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory,
            IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor)
        {
        }
    }
}
