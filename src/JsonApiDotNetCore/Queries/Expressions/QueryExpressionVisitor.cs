using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Implements the visitor design pattern that enables traversing a <see cref="QueryExpression" /> tree.
    /// </summary>
    [PublicAPI]
    public abstract class QueryExpressionVisitor<TArgument, TResult>
    {
        public virtual TResult Visit(QueryExpression expression, TArgument argument)
        {
            return expression.Accept(this, argument);
        }

        public virtual TResult DefaultVisit(QueryExpression expression, TArgument argument)
        {
            return default;
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

        public virtual TResult VisitCollectionNotEmpty(CollectionNotEmptyExpression expression, TArgument argument)
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

        public virtual TResult VisitEqualsAnyOf(EqualsAnyOfExpression expression, TArgument argument)
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

        public virtual TResult PaginationQueryStringValue(PaginationQueryStringValueExpression expression, TArgument argument)
        {
            return DefaultVisit(expression, argument);
        }

        public virtual TResult PaginationElementQueryStringValue(PaginationElementQueryStringValueExpression expression, TArgument argument)
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
}
