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
    public sealed class CarRepository : EntityFrameworkCoreRepository<Car, string>
    {
        private readonly CarExpressionRewriter _writer;

        public CarRepository(ITargetedFields targetedFields, IDbContextResolver contextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
            _writer = new CarExpressionRewriter(resourceGraph);
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
}
