using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Building block for rewriting <see cref="QueryExpression" /> trees. It walks through nested expressions and updates parent on changes.
    /// </summary>
    [PublicAPI]
    public class QueryExpressionRewriter<TArgument> : QueryExpressionVisitor<TArgument, QueryExpression>
    {
        public override QueryExpression Visit(QueryExpression expression, TArgument argument)
        {
            return expression.Accept(this, argument);
        }

        public override QueryExpression DefaultVisit(QueryExpression expression, TArgument argument)
        {
            return expression;
        }

        public override QueryExpression VisitComparison(ComparisonExpression expression, TArgument argument)
        {
            if (expression == null)
            {
                return null;
            }

            QueryExpression newLeft = Visit(expression.Left, argument);
            QueryExpression newRight = Visit(expression.Right, argument);

            var newExpression = new ComparisonExpression(expression.Operator, newLeft, newRight);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        public override QueryExpression VisitResourceFieldChain(ResourceFieldChainExpression expression, TArgument argument)
        {
            return expression;
        }

        public override QueryExpression VisitLiteralConstant(LiteralConstantExpression expression, TArgument argument)
        {
            return expression;
        }

        public override QueryExpression VisitNullConstant(NullConstantExpression expression, TArgument argument)
        {
            return expression;
        }

        public override QueryExpression VisitLogical(LogicalExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                IReadOnlyCollection<QueryExpression> newTerms = VisitSequence(expression.Terms, argument);

                if (newTerms.Count == 1)
                {
                    return newTerms.First();
                }

                if (newTerms.Count != 0)
                {
                    var newExpression = new LogicalExpression(expression.Operator, newTerms);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitNot(NotExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                QueryExpression newChild = Visit(expression.Child, argument);

                if (newChild != null)
                {
                    var newExpression = new NotExpression(newChild);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitCollectionNotEmpty(CollectionNotEmptyExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                if (Visit(expression.TargetCollection, argument) is ResourceFieldChainExpression newTargetCollection)
                {
                    var newExpression = new CollectionNotEmptyExpression(newTargetCollection);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitSortElement(SortElementExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                SortElementExpression newExpression = null;

                if (expression.Count != null)
                {
                    if (Visit(expression.Count, argument) is CountExpression newCount)
                    {
                        newExpression = new SortElementExpression(newCount, expression.IsAscending);
                    }
                }
                else if (expression.TargetAttribute != null)
                {
                    if (Visit(expression.TargetAttribute, argument) is ResourceFieldChainExpression newTargetAttribute)
                    {
                        newExpression = new SortElementExpression(newTargetAttribute, expression.IsAscending);
                    }
                }

                if (newExpression != null)
                {
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitSort(SortExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                IReadOnlyCollection<SortElementExpression> newElements = VisitSequence(expression.Elements, argument);

                if (newElements.Count != 0)
                {
                    var newExpression = new SortExpression(newElements);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitPagination(PaginationExpression expression, TArgument argument)
        {
            return expression;
        }

        public override QueryExpression VisitCount(CountExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                if (Visit(expression.TargetCollection, argument) is ResourceFieldChainExpression newTargetCollection)
                {
                    var newExpression = new CountExpression(newTargetCollection);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitMatchText(MatchTextExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                var newTargetAttribute = Visit(expression.TargetAttribute, argument) as ResourceFieldChainExpression;
                var newTextValue = Visit(expression.TextValue, argument) as LiteralConstantExpression;

                var newExpression = new MatchTextExpression(newTargetAttribute, newTextValue, expression.MatchKind);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitEqualsAnyOf(EqualsAnyOfExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                var newTargetAttribute = Visit(expression.TargetAttribute, argument) as ResourceFieldChainExpression;
                IReadOnlyCollection<LiteralConstantExpression> newConstants = VisitSequence(expression.Constants, argument);

                var newExpression = new EqualsAnyOfExpression(newTargetAttribute, newConstants);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitSparseFieldTable(SparseFieldTableExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                var newTable = new Dictionary<ResourceContext, SparseFieldSetExpression>();

                foreach ((ResourceContext resourceContext, SparseFieldSetExpression sparseFieldSet) in expression.Table)
                {
                    if (Visit(sparseFieldSet, argument) is SparseFieldSetExpression newSparseFieldSet)
                    {
                        newTable[resourceContext] = newSparseFieldSet;
                    }
                }

                if (newTable.Count > 0)
                {
                    var newExpression = new SparseFieldTableExpression(newTable);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitSparseFieldSet(SparseFieldSetExpression expression, TArgument argument)
        {
            return expression;
        }

        public override QueryExpression VisitQueryStringParameterScope(QueryStringParameterScopeExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                var newParameterName = Visit(expression.ParameterName, argument) as LiteralConstantExpression;

                ResourceFieldChainExpression newScope = expression.Scope != null ? Visit(expression.Scope, argument) as ResourceFieldChainExpression : null;

                var newExpression = new QueryStringParameterScopeExpression(newParameterName, newScope);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression PaginationQueryStringValue(PaginationQueryStringValueExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                IReadOnlyCollection<PaginationElementQueryStringValueExpression> newElements = VisitSequence(expression.Elements, argument);

                var newExpression = new PaginationQueryStringValueExpression(newElements);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression PaginationElementQueryStringValue(PaginationElementQueryStringValueExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                ResourceFieldChainExpression newScope = expression.Scope != null ? Visit(expression.Scope, argument) as ResourceFieldChainExpression : null;

                var newExpression = new PaginationElementQueryStringValueExpression(newScope, expression.Value);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitInclude(IncludeExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                IReadOnlyCollection<IncludeElementExpression> newElements = VisitSequence(expression.Elements, argument);

                if (newElements.Count == 0)
                {
                    return IncludeExpression.Empty;
                }

                var newExpression = new IncludeExpression(newElements);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitIncludeElement(IncludeElementExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                IReadOnlyCollection<IncludeElementExpression> newElements = VisitSequence(expression.Children, argument);

                var newExpression = new IncludeElementExpression(expression.Relationship, newElements);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitQueryableHandler(QueryableHandlerExpression expression, TArgument argument)
        {
            return expression;
        }

        protected virtual IReadOnlyCollection<TExpression> VisitSequence<TExpression>(IEnumerable<TExpression> elements, TArgument argument)
            where TExpression : QueryExpression
        {
            var newElements = new List<TExpression>();

            foreach (TExpression element in elements)
            {
                if (Visit(element, argument) is TExpression newElement)
                {
                    newElements.Add(newElement);
                }
            }

            return newElements;
        }
    }
}
