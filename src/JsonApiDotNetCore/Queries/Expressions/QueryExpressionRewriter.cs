using System.Collections.Immutable;
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
                IImmutableList<FilterExpression> newTerms = VisitList(expression.Terms, argument);

                if (newTerms.Count == 1)
                {
                    return newTerms[0];
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
                if (Visit(expression.Child, argument) is FilterExpression newChild)
                {
                    var newExpression = new NotExpression(newChild);
                    return newExpression.Equals(expression) ? expression : newExpression;
                }
            }

            return null;
        }

        public override QueryExpression VisitHas(HasExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                if (Visit(expression.TargetCollection, argument) is ResourceFieldChainExpression newTargetCollection)
                {
                    FilterExpression newFilter = expression.Filter != null ? Visit(expression.Filter, argument) as FilterExpression : null;

                    var newExpression = new HasExpression(newTargetCollection, newFilter);
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
                IImmutableList<SortElementExpression> newElements = VisitList(expression.Elements, argument);

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

        public override QueryExpression VisitAny(AnyExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                var newTargetAttribute = Visit(expression.TargetAttribute, argument) as ResourceFieldChainExpression;
                IImmutableSet<LiteralConstantExpression> newConstants = VisitSet(expression.Constants, argument);

                var newExpression = new AnyExpression(newTargetAttribute, newConstants);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitSparseFieldTable(SparseFieldTableExpression expression, TArgument argument)
        {
            if (expression != null)
            {
                ImmutableDictionary<ResourceContext, SparseFieldSetExpression>.Builder newTable =
                    ImmutableDictionary.CreateBuilder<ResourceContext, SparseFieldSetExpression>();

                foreach ((ResourceContext resourceContext, SparseFieldSetExpression sparseFieldSet) in expression.Table)
                {
                    if (Visit(sparseFieldSet, argument) is SparseFieldSetExpression newSparseFieldSet)
                    {
                        newTable[resourceContext] = newSparseFieldSet;
                    }
                }

                if (newTable.Count > 0)
                {
                    var newExpression = new SparseFieldTableExpression(newTable.ToImmutable());
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
                IImmutableList<PaginationElementQueryStringValueExpression> newElements = VisitList(expression.Elements, argument);

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
                IImmutableList<IncludeElementExpression> newElements = VisitList(expression.Elements, argument);

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
                IImmutableList<IncludeElementExpression> newElements = VisitList(expression.Children, argument);

                var newExpression = new IncludeElementExpression(expression.Relationship, newElements);
                return newExpression.Equals(expression) ? expression : newExpression;
            }

            return null;
        }

        public override QueryExpression VisitQueryableHandler(QueryableHandlerExpression expression, TArgument argument)
        {
            return expression;
        }

        protected virtual IImmutableList<TExpression> VisitList<TExpression>(IImmutableList<TExpression> elements, TArgument argument)
            where TExpression : QueryExpression
        {
            ImmutableArray<TExpression>.Builder arrayBuilder = ImmutableArray.CreateBuilder<TExpression>(elements.Count);

            foreach (TExpression element in elements)
            {
                if (Visit(element, argument) is TExpression newElement)
                {
                    arrayBuilder.Add(newElement);
                }
            }

            return arrayBuilder.ToImmutable();
        }

        protected virtual IImmutableSet<TExpression> VisitSet<TExpression>(IImmutableSet<TExpression> elements, TArgument argument)
            where TExpression : QueryExpression
        {
            ImmutableHashSet<TExpression>.Builder setBuilder = ImmutableHashSet.CreateBuilder<TExpression>();

            foreach (TExpression element in elements)
            {
                if (Visit(element, argument) is TExpression newElement)
                {
                    setBuilder.Add(newElement);
                }
            }

            return setBuilder.ToImmutable();
        }
    }
}
