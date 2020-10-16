using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    /// <summary>
    /// Rewrites an expression tree, updating all references to <see cref="Car.Id"/> with
    /// the combination of <see cref="Car.RegionId"/> and <see cref="Car.LicensePlate"/>.
    /// </summary>
    /// <remarks>
    /// This enables queries to use <see cref="Car.Id"/>, which is not mapped in the database.
    /// </remarks>
    public sealed class CarExpressionRewriter : QueryExpressionRewriter<object>
    {
        private readonly AttrAttribute _regionIdAttribute;
        private readonly AttrAttribute _licensePlateAttribute;

        public CarExpressionRewriter(IResourceContextProvider resourceContextProvider)
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

                    return RewriteFilterOnCarStringIds(leftChain, new[] {rightConstant.Value});
                }
            }

            return base.VisitComparison(expression, argument);
        }

        public override QueryExpression VisitEqualsAnyOf(EqualsAnyOfExpression expression, object argument)
        {
            PropertyInfo property = expression.TargetAttribute.Fields.Last().Property;
            if (IsCarId(property))
            {
                var carStringIds = expression.Constants.Select(constant => constant.Value).ToArray();
                return RewriteFilterOnCarStringIds(expression.TargetAttribute, carStringIds);
            }

            return base.VisitEqualsAnyOf(expression, argument);
        }

        public override QueryExpression VisitMatchText(MatchTextExpression expression, object argument)
        {
            PropertyInfo property = expression.TargetAttribute.Fields.Last().Property;
            if (IsCarId(property))
            {
                throw new NotSupportedException("Partial text matching on Car IDs is not possible.");
            }

            return base.VisitMatchText(expression, argument);
        }

        private static bool IsCarId(PropertyInfo property)
        {
            return property.Name == nameof(Identifiable.Id) && property.DeclaringType == typeof(Car);
        }

        private QueryExpression RewriteFilterOnCarStringIds(ResourceFieldChainExpression existingCarIdChain,
            IEnumerable<string> carStringIds)
        {
            var outerTerms = new List<QueryExpression>();

            foreach (var carStringId in carStringIds)
            {
                var tempCar = new Car
                {
                    StringId = carStringId
                };

                var keyComparison =
                    CreateEqualityComparisonOnCompositeKey(existingCarIdChain, tempCar.RegionId, tempCar.LicensePlate);
                outerTerms.Add(keyComparison);
            }

            return outerTerms.Count == 1 ? outerTerms[0] : new LogicalExpression(LogicalOperator.Or, outerTerms);
        }

        private QueryExpression CreateEqualityComparisonOnCompositeKey(ResourceFieldChainExpression existingCarIdChain,
            long regionIdValue, string licensePlateValue)
        {
            var regionIdChain = ReplaceLastAttributeInChain(existingCarIdChain, _regionIdAttribute);
            var regionIdComparison = new ComparisonExpression(ComparisonOperator.Equals, regionIdChain,
                new LiteralConstantExpression(regionIdValue.ToString()));

            var licensePlateChain = ReplaceLastAttributeInChain(existingCarIdChain, _licensePlateAttribute);
            var licensePlateComparison = new ComparisonExpression(ComparisonOperator.Equals, licensePlateChain,
                new LiteralConstantExpression(licensePlateValue));

            return new LogicalExpression(LogicalOperator.And, new[]
            {
                regionIdComparison,
                licensePlateComparison
            });
        }

        public override QueryExpression VisitSort(SortExpression expression, object argument)
        {
            var newSortElements = new List<SortElementExpression>();

            foreach (var sortElement in expression.Elements)
            {
                if (IsSortOnCarId(sortElement))
                {
                    var regionIdSort = ReplaceLastAttributeInChain(sortElement.TargetAttribute, _regionIdAttribute);
                    newSortElements.Add(new SortElementExpression(regionIdSort, sortElement.IsAscending));

                    var licensePlateSort =
                        ReplaceLastAttributeInChain(sortElement.TargetAttribute, _licensePlateAttribute);
                    newSortElements.Add(new SortElementExpression(licensePlateSort, sortElement.IsAscending));
                }
                else
                {
                    newSortElements.Add(sortElement);
                }
            }

            return new SortExpression(newSortElements);
        }

        private static bool IsSortOnCarId(SortElementExpression sortElement)
        {
            if (sortElement.TargetAttribute != null)
            {
                PropertyInfo property = sortElement.TargetAttribute.Fields.Last().Property;
                if (IsCarId(property))
                {
                    return true;
                }
            }

            return false;
        }

        private static ResourceFieldChainExpression ReplaceLastAttributeInChain(
            ResourceFieldChainExpression resourceFieldChain, AttrAttribute attribute)
        {
            var fields = resourceFieldChain.Fields.ToList();
            fields[^1] = attribute;
            return new ResourceFieldChainExpression(fields);
        }
    }
}
