using System.ComponentModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
// The constructor parameters will be resolved from the container, which means you can take on any dependency that is also defined in the container.
public sealed class StarDefinition(IResourceGraph resourceGraph, IClientSettingsProvider clientSettingsProvider, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Star, long>(resourceGraph, hitCounter)
{
    private readonly IClientSettingsProvider _clientSettingsProvider = clientSettingsProvider;

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

    public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
    {
        FilterExpression? baseFilter = base.OnApplyFilter(existingFilter);

        if (_clientSettingsProvider.AreVeryLargeStarsHidden)
        {
            AttrAttribute solarRadiusAttribute = ResourceType.GetAttributeByPropertyName(nameof(Star.SolarRadius));
            var solarRadiusChain = new ResourceFieldChainExpression(solarRadiusAttribute);
            var solarRadiusComparison = new ComparisonExpression(ComparisonOperator.LessThan, solarRadiusChain, new LiteralConstantExpression(2000M));

            return LogicalExpression.Compose(LogicalOperator.And, baseFilter, solarRadiusComparison);
        }

        return baseFilter;
    }

    public override SortExpression OnApplySort(SortExpression? existingSort)
    {
        base.OnApplySort(existingSort);

        return existingSort ?? GetDefaultSortOrder();
    }

    private SortExpression GetDefaultSortOrder()
    {
        return CreateSortExpressionFromLambda([
            (star => star.SolarMass, ListSortDirection.Descending),
            (star => star.SolarRadius, ListSortDirection.Descending)
        ]);
    }

    public override PaginationExpression OnApplyPagination(PaginationExpression? existingPagination)
    {
        base.OnApplyPagination(existingPagination);

        var maxPageSize = new PageSize(5);

        if (existingPagination != null)
        {
            PageSize pageSize = existingPagination.PageSize?.Value <= maxPageSize.Value ? existingPagination.PageSize : maxPageSize;
            return new PaginationExpression(existingPagination.PageNumber, pageSize);
        }

        return new PaginationExpression(PageNumber.ValueOne, maxPageSize);
    }

    public override SparseFieldSetExpression? OnApplySparseFieldSet(SparseFieldSetExpression? existingSparseFieldSet)
    {
        base.OnApplySparseFieldSet(existingSparseFieldSet);

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        return existingSparseFieldSet
            .Including<Star>(star => star.Kind, ResourceGraph)
            .Excluding<Star>(star => star.IsVisibleFromEarth, ResourceGraph);

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore
    }
}
