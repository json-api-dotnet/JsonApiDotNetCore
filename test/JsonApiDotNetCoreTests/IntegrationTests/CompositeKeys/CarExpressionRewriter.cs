using System.Collections.Immutable;
using System.Reflection;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

/// <summary>
/// Rewrites an expression tree, updating all references to <see cref="Car.Id" /> with the combination of <see cref="Car.RegionId" /> and
/// <see cref="Car.LicensePlate" />.
/// </summary>
/// <remarks>
/// This enables queries to use <see cref="Car.Id" />, which is not mapped in the database.
/// </remarks>
internal sealed class CarExpressionRewriter : QueryExpressionRewriter<object?>
{
    private readonly AttrAttribute _regionIdAttribute;
    private readonly AttrAttribute _licensePlateAttribute;

    public CarExpressionRewriter(IResourceGraph resourceGraph)
    {
        ResourceType carType = resourceGraph.GetResourceType<Car>();

        _regionIdAttribute = carType.GetAttributeByPropertyName(nameof(Car.RegionId));
        _licensePlateAttribute = carType.GetAttributeByPropertyName(nameof(Car.LicensePlate));
    }

    public override QueryExpression? VisitComparison(ComparisonExpression expression, object? argument)
    {
        if (expression.Left is ResourceFieldChainExpression leftChain && expression.Right is LiteralConstantExpression rightConstant)
        {
            PropertyInfo leftProperty = leftChain.Fields[^1].Property;

            if (IsCarId(leftProperty))
            {
                if (expression.Operator != ComparisonOperator.Equals)
                {
                    throw new NotSupportedException("Only equality comparisons are possible on Car IDs.");
                }

                return RewriteFilterOnCarStringIds(leftChain, rightConstant.Value.AsEnumerable());
            }
        }

        return base.VisitComparison(expression, argument);
    }

    public override QueryExpression? VisitAny(AnyExpression expression, object? argument)
    {
        PropertyInfo property = expression.TargetAttribute.Fields[^1].Property;

        if (IsCarId(property))
        {
            string[] carStringIds = expression.Constants.Select(constant => constant.Value).ToArray();
            return RewriteFilterOnCarStringIds(expression.TargetAttribute, carStringIds);
        }

        return base.VisitAny(expression, argument);
    }

    public override QueryExpression? VisitMatchText(MatchTextExpression expression, object? argument)
    {
        PropertyInfo property = expression.TargetAttribute.Fields[^1].Property;

        if (IsCarId(property))
        {
            throw new NotSupportedException("Partial text matching on Car IDs is not possible.");
        }

        return base.VisitMatchText(expression, argument);
    }

    private static bool IsCarId(PropertyInfo property)
    {
        return property.Name == nameof(Identifiable<object>.Id) && property.DeclaringType == typeof(Car);
    }

    private QueryExpression RewriteFilterOnCarStringIds(ResourceFieldChainExpression existingCarIdChain, IEnumerable<string> carStringIds)
    {
        ImmutableArray<FilterExpression>.Builder outerTermsBuilder = ImmutableArray.CreateBuilder<FilterExpression>();

        foreach (string carStringId in carStringIds)
        {
            var tempCar = new Car
            {
                StringId = carStringId
            };

            FilterExpression keyComparison = CreateEqualityComparisonOnCompositeKey(existingCarIdChain, tempCar.RegionId, tempCar.LicensePlate!);
            outerTermsBuilder.Add(keyComparison);
        }

        return outerTermsBuilder.Count == 1 ? outerTermsBuilder[0] : new LogicalExpression(LogicalOperator.Or, outerTermsBuilder.ToImmutable());
    }

    private FilterExpression CreateEqualityComparisonOnCompositeKey(ResourceFieldChainExpression existingCarIdChain, long regionIdValue,
        string licensePlateValue)
    {
        ResourceFieldChainExpression regionIdChain = ReplaceLastAttributeInChain(existingCarIdChain, _regionIdAttribute);
        var regionIdComparison = new ComparisonExpression(ComparisonOperator.Equals, regionIdChain, new LiteralConstantExpression(regionIdValue.ToString()));

        ResourceFieldChainExpression licensePlateChain = ReplaceLastAttributeInChain(existingCarIdChain, _licensePlateAttribute);
        var licensePlateComparison = new ComparisonExpression(ComparisonOperator.Equals, licensePlateChain, new LiteralConstantExpression(licensePlateValue));

        return new LogicalExpression(LogicalOperator.And, regionIdComparison, licensePlateComparison);
    }

    public override QueryExpression VisitSort(SortExpression expression, object? argument)
    {
        ImmutableArray<SortElementExpression>.Builder elementsBuilder = ImmutableArray.CreateBuilder<SortElementExpression>(expression.Elements.Count);

        foreach (SortElementExpression sortElement in expression.Elements)
        {
            if (IsSortOnCarId(sortElement))
            {
                ResourceFieldChainExpression regionIdSort = ReplaceLastAttributeInChain(sortElement.TargetAttribute!, _regionIdAttribute);
                elementsBuilder.Add(new SortElementExpression(regionIdSort, sortElement.IsAscending));

                ResourceFieldChainExpression licensePlateSort = ReplaceLastAttributeInChain(sortElement.TargetAttribute!, _licensePlateAttribute);
                elementsBuilder.Add(new SortElementExpression(licensePlateSort, sortElement.IsAscending));
            }
            else
            {
                elementsBuilder.Add(sortElement);
            }
        }

        return new SortExpression(elementsBuilder.ToImmutable());
    }

    private static bool IsSortOnCarId(SortElementExpression sortElement)
    {
        if (sortElement.TargetAttribute != null)
        {
            PropertyInfo property = sortElement.TargetAttribute.Fields[^1].Property;

            if (IsCarId(property))
            {
                return true;
            }
        }

        return false;
    }

    private static ResourceFieldChainExpression ReplaceLastAttributeInChain(ResourceFieldChainExpression resourceFieldChain, AttrAttribute attribute)
    {
        IImmutableList<ResourceFieldAttribute> fields = resourceFieldChain.Fields.SetItem(resourceFieldChain.Fields.Count - 1, attribute);
        return new ResourceFieldChainExpression(fields);
    }
}
