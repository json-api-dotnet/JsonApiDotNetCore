using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class CarRepository : EntityFrameworkCoreRepository<Car, string>
    {
        private readonly IResourceGraph _resourceGraph;

        public CarRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver,
            IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
            _resourceGraph = resourceGraph;
        }

        protected override IQueryable<Car> ApplyQueryLayer(QueryLayer layer)
        {
            RecursiveRewriteFilterInLayer(layer);

            return base.ApplyQueryLayer(layer);
        }

        private void RecursiveRewriteFilterInLayer(QueryLayer queryLayer)
        {
            if (queryLayer.Filter != null)
            {
                var writer = new CarExpressionRewriter(_resourceGraph);
                queryLayer.Filter = (FilterExpression) writer.Visit(queryLayer.Filter, null);
            }

            if (queryLayer.Sort != null)
            {
                var writer = new CarExpressionRewriter(_resourceGraph);
                queryLayer.Sort = (SortExpression) writer.Visit(queryLayer.Sort, null);
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
}
