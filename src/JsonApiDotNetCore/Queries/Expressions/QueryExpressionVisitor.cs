using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Implements the visitor design pattern that enables traversing a <see cref="QueryExpression" /> tree.
/// </summary>
/// <typeparam name="TArgument">
/// The type to use for passing custom state between visit methods.
/// </typeparam>
/// <typeparam name="TResult">
/// The type that is returned from visit methods.
/// </typeparam>
[PublicAPI]
public abstract class QueryExpressionVisitor<TArgument, TResult>
{
    public virtual TResult Visit(QueryExpression expression, TArgument argument)
    {
        return expression.Accept(this, argument);
    }

    public virtual TResult DefaultVisit(QueryExpression expression, TArgument argument)
    {
        return default!;
    }

    public virtual TResult VisitComparison(ComparisonExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitResourceFieldChain(ResourceFieldChainExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitLiteralConstant(LiteralConstantExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitNullConstant(NullConstantExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitLogical(LogicalExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitNot(NotExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitHas(HasExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitIsType(IsTypeExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitSortElement(SortElementExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitSort(SortExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitPagination(PaginationExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitCount(CountExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitMatchText(MatchTextExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitAny(AnyExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitSparseFieldTable(SparseFieldTableExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitSparseFieldSet(SparseFieldSetExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitQueryStringParameterScope(QueryStringParameterScopeExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitPaginationQueryStringValue(PaginationQueryStringValueExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitPaginationElementQueryStringValue(PaginationElementQueryStringValueExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitInclude(IncludeExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitIncludeElement(IncludeElementExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }

    public virtual TResult VisitQueryableHandler(QueryableHandlerExpression expression, TArgument argument)
    {
        return DefaultVisit(expression, argument);
    }
}
