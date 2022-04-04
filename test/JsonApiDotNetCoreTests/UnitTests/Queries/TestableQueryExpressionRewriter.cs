using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.UnitTests.Queries;

internal sealed class TestableQueryExpressionRewriter : QueryExpressionRewriter<object?>
{
    public List<QueryExpression> ExpressionsVisited { get; } = new();

    public override QueryExpression DefaultVisit(QueryExpression expression, object? argument)
    {
        Capture(expression);
        return base.DefaultVisit(expression, argument);
    }

    public override QueryExpression? VisitComparison(ComparisonExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitComparison(expression, argument);
    }

    public override QueryExpression? VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitResourceFieldChain(expression, argument);
    }

    public override QueryExpression VisitLiteralConstant(LiteralConstantExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitLiteralConstant(expression, argument);
    }

    public override QueryExpression VisitNullConstant(NullConstantExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitNullConstant(expression, argument);
    }

    public override QueryExpression? VisitLogical(LogicalExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitLogical(expression, argument);
    }

    public override QueryExpression? VisitNot(NotExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitNot(expression, argument);
    }

    public override QueryExpression? VisitHas(HasExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitHas(expression, argument);
    }

    public override QueryExpression VisitIsType(IsTypeExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitIsType(expression, argument);
    }

    public override QueryExpression? VisitSortElement(SortElementExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitSortElement(expression, argument);
    }

    public override QueryExpression? VisitSort(SortExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitSort(expression, argument);
    }

    public override QueryExpression VisitPagination(PaginationExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitPagination(expression, argument);
    }

    public override QueryExpression? VisitCount(CountExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitCount(expression, argument);
    }

    public override QueryExpression? VisitMatchText(MatchTextExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitMatchText(expression, argument);
    }

    public override QueryExpression? VisitAny(AnyExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitAny(expression, argument);
    }

    public override QueryExpression? VisitSparseFieldTable(SparseFieldTableExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitSparseFieldTable(expression, argument);
    }

    public override QueryExpression VisitSparseFieldSet(SparseFieldSetExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitSparseFieldSet(expression, argument);
    }

    public override QueryExpression? VisitQueryStringParameterScope(QueryStringParameterScopeExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitQueryStringParameterScope(expression, argument);
    }

    public override QueryExpression PaginationQueryStringValue(PaginationQueryStringValueExpression expression, object? argument)
    {
        Capture(expression);
        return base.PaginationQueryStringValue(expression, argument);
    }

    public override QueryExpression PaginationElementQueryStringValue(PaginationElementQueryStringValueExpression expression, object? argument)
    {
        Capture(expression);
        return base.PaginationElementQueryStringValue(expression, argument);
    }

    public override QueryExpression VisitInclude(IncludeExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitInclude(expression, argument);
    }

    public override QueryExpression VisitIncludeElement(IncludeElementExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitIncludeElement(expression, argument);
    }

    public override QueryExpression VisitQueryableHandler(QueryableHandlerExpression expression, object? argument)
    {
        Capture(expression);
        return base.VisitQueryableHandler(expression, argument);
    }

    private void Capture(QueryExpression expression)
    {
        ExpressionsVisited.Add(expression);
    }
}
