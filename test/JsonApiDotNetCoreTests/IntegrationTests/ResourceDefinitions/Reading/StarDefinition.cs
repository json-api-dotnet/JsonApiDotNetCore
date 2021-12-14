using System.ComponentModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class StarDefinition : HitCountingResourceDefinition<Star, int>
{
    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Reading;

    public StarDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
        : base(resourceGraph, hitCounter)
    {
        // This constructor will be resolved from the container, which means
        // you can take on any dependency that is also defined in the container.
    }

    public override SortExpression OnApplySort(SortExpression? existingSort)
    {
        base.OnApplySort(existingSort);

        return existingSort ?? GetDefaultSortOrder();
    }

    private SortExpression GetDefaultSortOrder()
    {
        return CreateSortExpressionFromLambda(new PropertySortOrder
        {
            (star => star.SolarMass, ListSortDirection.Descending),
            (star => star.SolarRadius, ListSortDirection.Descending)
        });
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

        // @formatter:keep_existing_linebreaks true

        return existingSparseFieldSet
            .Including<Star>(star => star.Kind, ResourceGraph)
            .Excluding<Star>(star => star.IsVisibleFromEarth, ResourceGraph);

        // @formatter:keep_existing_linebreaks restore
    }
}
