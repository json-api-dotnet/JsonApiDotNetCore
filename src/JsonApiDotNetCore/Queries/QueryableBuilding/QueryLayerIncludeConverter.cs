using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Replaces all <see cref="QueryLayer.Include" />s with <see cref="QueryLayer.Selection" />s in-place.
/// </summary>
public sealed class QueryLayerIncludeConverter : QueryExpressionVisitor<QueryLayer, object?>
{
    private readonly QueryLayer _queryLayer;

    public QueryLayerIncludeConverter(QueryLayer queryLayer)
    {
        ArgumentGuard.NotNull(queryLayer);

        _queryLayer = queryLayer;
    }

    public void ConvertIncludesToSelections()
    {
        if (_queryLayer.Include != null)
        {
            Visit(_queryLayer.Include, _queryLayer);
            _queryLayer.Include = null;
        }

        EnsureNonEmptySelection(_queryLayer);
    }

    public override object? VisitInclude(IncludeExpression expression, QueryLayer queryLayer)
    {
        foreach (IncludeElementExpression element in expression.Elements.OrderBy(element => element.Relationship.PublicName))
        {
            _ = Visit(element, queryLayer);
        }

        return null;
    }

    public override object? VisitIncludeElement(IncludeElementExpression expression, QueryLayer queryLayer)
    {
        QueryLayer subLayer = EnsureRelationshipInSelection(queryLayer, expression.Relationship);

        foreach (IncludeElementExpression nextIncludeElement in expression.Children.OrderBy(child => child.Relationship.PublicName))
        {
            Visit(nextIncludeElement, subLayer);
        }

        return null;
    }

    private static QueryLayer EnsureRelationshipInSelection(QueryLayer queryLayer, RelationshipAttribute relationship)
    {
        queryLayer.Selection ??= new FieldSelection();
        FieldSelectors selectors = queryLayer.Selection.GetOrCreateSelectors(queryLayer.ResourceType);

        if (!selectors.ContainsField(relationship))
        {
            selectors.IncludeRelationship(relationship, new QueryLayer(relationship.RightType));
        }

        QueryLayer subLayer = selectors[relationship]!;
        EnsureNonEmptySelection(subLayer);

        return subLayer;
    }

    private static void EnsureNonEmptySelection(QueryLayer queryLayer)
    {
        if (queryLayer.Selection == null)
        {
            // Empty selection indicates to fetch all scalar properties.
            queryLayer.Selection = new FieldSelection();
            queryLayer.Selection.GetOrCreateSelectors(queryLayer.ResourceType);
        }
    }
}
