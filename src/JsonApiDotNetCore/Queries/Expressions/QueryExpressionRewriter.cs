using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Building block for rewriting <see cref="QueryExpression" /> trees. It walks through nested expressions and updates the parent on changes.
/// </summary>
[PublicAPI]
public class QueryExpressionRewriter<TArgument> : QueryExpressionVisitor<TArgument, QueryExpression?>
{
    public override QueryExpression? Visit(QueryExpression expression, TArgument argument)
    {
        return expression.Accept(this, argument);
    }

    public override QueryExpression DefaultVisit(QueryExpression expression, TArgument argument)
    {
        return expression;
    }

    public override QueryExpression? VisitComparison(ComparisonExpression expression, TArgument argument)
    {
        QueryExpression? newLeft = Visit(expression.Left, argument);
        QueryExpression? newRight = Visit(expression.Right, argument);

        if (newLeft != null && newRight != null)
        {
            var newExpression = new ComparisonExpression(expression.Operator, newLeft, newRight);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression? VisitResourceFieldChain(ResourceFieldChainExpression expression, TArgument argument)
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

    public override QueryExpression? VisitLogical(LogicalExpression expression, TArgument argument)
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

        return null;
    }

    public override QueryExpression? VisitNot(NotExpression expression, TArgument argument)
    {
        if (Visit(expression.Child, argument) is FilterExpression newChild)
        {
            var newExpression = new NotExpression(newChild);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression? VisitHas(HasExpression expression, TArgument argument)
    {
        if (Visit(expression.TargetCollection, argument) is ResourceFieldChainExpression newTargetCollection)
        {
            FilterExpression? newFilter = expression.Filter != null ? Visit(expression.Filter, argument) as FilterExpression : null;

            var newExpression = new HasExpression(newTargetCollection, newFilter);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression VisitIsType(IsTypeExpression expression, TArgument argument)
    {
        ResourceFieldChainExpression? newTargetToOneRelationship = expression.TargetToOneRelationship != null
            ? Visit(expression.TargetToOneRelationship, argument) as ResourceFieldChainExpression
            : null;

        FilterExpression? newChild = expression.Child != null ? Visit(expression.Child, argument) as FilterExpression : null;

        var newExpression = new IsTypeExpression(newTargetToOneRelationship, expression.DerivedType, newChild);
        return newExpression.Equals(expression) ? expression : newExpression;
    }

    public override QueryExpression? VisitSortElement(SortElementExpression expression, TArgument argument)
    {
        QueryExpression? newTarget = Visit(expression.Target, argument);

        if (newTarget != null)
        {
            var newExpression = new SortElementExpression(newTarget, expression.IsAscending);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression? VisitSort(SortExpression expression, TArgument argument)
    {
        IImmutableList<SortElementExpression> newElements = VisitList(expression.Elements, argument);

        if (newElements.Count != 0)
        {
            var newExpression = new SortExpression(newElements);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression VisitPagination(PaginationExpression expression, TArgument argument)
    {
        return expression;
    }

    public override QueryExpression? VisitCount(CountExpression expression, TArgument argument)
    {
        if (Visit(expression.TargetCollection, argument) is ResourceFieldChainExpression newTargetCollection)
        {
            var newExpression = new CountExpression(newTargetCollection);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression? VisitMatchText(MatchTextExpression expression, TArgument argument)
    {
        var newTargetAttribute = Visit(expression.TargetAttribute, argument) as ResourceFieldChainExpression;
        var newTextValue = Visit(expression.TextValue, argument) as LiteralConstantExpression;

        if (newTargetAttribute != null && newTextValue != null)
        {
            var newExpression = new MatchTextExpression(newTargetAttribute, newTextValue, expression.MatchKind);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression? VisitAny(AnyExpression expression, TArgument argument)
    {
        var newTargetAttribute = Visit(expression.TargetAttribute, argument) as ResourceFieldChainExpression;
        IImmutableSet<LiteralConstantExpression> newConstants = VisitSet(expression.Constants, argument);

        if (newTargetAttribute != null)
        {
            var newExpression = new AnyExpression(newTargetAttribute, newConstants);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression? VisitSparseFieldTable(SparseFieldTableExpression expression, TArgument argument)
    {
        ImmutableDictionary<ResourceType, SparseFieldSetExpression>.Builder newTable =
            ImmutableDictionary.CreateBuilder<ResourceType, SparseFieldSetExpression>();

        foreach ((ResourceType resourceType, SparseFieldSetExpression sparseFieldSet) in expression.Table)
        {
            if (Visit(sparseFieldSet, argument) is SparseFieldSetExpression newSparseFieldSet)
            {
                newTable[resourceType] = newSparseFieldSet;
            }
        }

        if (newTable.Count > 0)
        {
            var newExpression = new SparseFieldTableExpression(newTable.ToImmutable());
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression VisitSparseFieldSet(SparseFieldSetExpression expression, TArgument argument)
    {
        return expression;
    }

    public override QueryExpression? VisitQueryStringParameterScope(QueryStringParameterScopeExpression expression, TArgument argument)
    {
        var newParameterName = Visit(expression.ParameterName, argument) as LiteralConstantExpression;
        ResourceFieldChainExpression? newScope = expression.Scope != null ? Visit(expression.Scope, argument) as ResourceFieldChainExpression : null;

        if (newParameterName != null)
        {
            var newExpression = new QueryStringParameterScopeExpression(newParameterName, newScope);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression PaginationQueryStringValue(PaginationQueryStringValueExpression expression, TArgument argument)
    {
        IImmutableList<PaginationElementQueryStringValueExpression> newElements = VisitList(expression.Elements, argument);

        var newExpression = new PaginationQueryStringValueExpression(newElements);
        return newExpression.Equals(expression) ? expression : newExpression;
    }

    public override QueryExpression PaginationElementQueryStringValue(PaginationElementQueryStringValueExpression expression, TArgument argument)
    {
        ResourceFieldChainExpression? newScope = expression.Scope != null ? Visit(expression.Scope, argument) as ResourceFieldChainExpression : null;

        var newExpression = new PaginationElementQueryStringValueExpression(newScope, expression.Value, expression.Position);
        return newExpression.Equals(expression) ? expression : newExpression;
    }

    public override QueryExpression VisitInclude(IncludeExpression expression, TArgument argument)
    {
        IImmutableSet<IncludeElementExpression> newElements = VisitSet(expression.Elements, argument);

        if (newElements.Count == 0)
        {
            return IncludeExpression.Empty;
        }

        var newExpression = new IncludeExpression(newElements);
        return newExpression.Equals(expression) ? expression : newExpression;
    }

    public override QueryExpression VisitIncludeElement(IncludeElementExpression expression, TArgument argument)
    {
        IImmutableSet<IncludeElementExpression> newElements = VisitSet(expression.Children, argument);

        var newExpression = new IncludeElementExpression(expression.Relationship, newElements);
        return newExpression.Equals(expression) ? expression : newExpression;
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
