using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Repositories
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
                var writer = new CarFilterRewriter(_resourceGraph);
                queryLayer.Filter = (FilterExpression) writer.Visit(queryLayer.Filter, null);
            }

            if (queryLayer.Projection != null)
            {
                foreach (QueryLayer nextLayer in queryLayer.Projection.Values.Where(layer => layer != null))
                {
                    RecursiveRewriteFilterInLayer(nextLayer);
                }
            }
        }

        private sealed class CarFilterRewriter : QueryExpressionRewriter<object>
        {
            private readonly AttrAttribute _regionIdAttribute;
            private readonly AttrAttribute _licensePlateAttribute;

            public CarFilterRewriter(IResourceContextProvider resourceContextProvider)
            {
                var carResourceContext = resourceContextProvider.GetResourceContext<Car>();

                _regionIdAttribute =
                    carResourceContext.Attributes.Single(attribute =>
                        attribute.Property.Name == nameof(Car.RegionId));

                _licensePlateAttribute =
                    carResourceContext.Attributes.Single(attribute =>
                        attribute.Property.Name == nameof(Car.LicensePlate));
            }

            public override QueryExpression VisitComparison(ComparisonExpression expression, object argument)
            {
                if (expression.Left is ResourceFieldChainExpression leftChain &&
                    expression.Right is LiteralConstantExpression rightConstant)
                {
                    PropertyInfo leftProperty = leftChain.Fields.Last().Property;
                    if (IsCarId(leftProperty))
                    {
                        if (expression.Operator != ComparisonOperator.Equals)
                        {
                            throw new NotSupportedException("Only equality comparisons are possible on Car IDs.");
                        }

                        return RewriteEqualityComparisonForCarStringId(rightConstant.Value);
                    }
                }

                return base.VisitComparison(expression, argument);
            }

            private static bool IsCarId(PropertyInfo property)
            {
                return property.Name == nameof(Identifiable.Id) && property.DeclaringType == typeof(Car);
            }

            private QueryExpression RewriteEqualityComparisonForCarStringId(string carStringId)
            {
                var tempCar = new Car
                {
                    StringId = carStringId
                };

                return CreateEqualityComparisonOnRegionIdLicensePlate(tempCar.RegionId, tempCar.LicensePlate);
            }

            private QueryExpression CreateEqualityComparisonOnRegionIdLicensePlate(long? regionIdValue,
                string licensePlateValue)
            {
                var regionIdComparison = new ComparisonExpression(ComparisonOperator.Equals,
                    new ResourceFieldChainExpression(_regionIdAttribute),
                    new LiteralConstantExpression(regionIdValue.ToString()));

                var licensePlateComparison = new ComparisonExpression(ComparisonOperator.Equals,
                    new ResourceFieldChainExpression(_licensePlateAttribute),
                    new LiteralConstantExpression(licensePlateValue));

                return new LogicalExpression(LogicalOperator.And, new[]
                {
                    regionIdComparison,
                    licensePlateComparison
                });
            }
        }
    }
}
