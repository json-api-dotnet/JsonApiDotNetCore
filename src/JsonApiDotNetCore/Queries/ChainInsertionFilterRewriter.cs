using System.Collections.Immutable;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

internal sealed class ChainInsertionFilterRewriter : QueryExpressionRewriter<object?>
{
    private readonly ResourceFieldAttribute _fieldToInsert;
    private bool _isInNestedScope;

    public ChainInsertionFilterRewriter(ResourceFieldAttribute fieldToInsert)
    {
        ArgumentNullException.ThrowIfNull(fieldToInsert);

        _fieldToInsert = fieldToInsert;
    }

    public override QueryExpression VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
    {
        if (_isInNestedScope)
        {
            return expression;
        }

        IImmutableList<ResourceFieldAttribute> newFields = expression.Fields.Insert(0, _fieldToInsert);
        return new ResourceFieldChainExpression(newFields);
    }

    public override QueryExpression? VisitHas(HasExpression expression, object? argument)
    {
        if (Visit(expression.TargetCollection, argument) is ResourceFieldChainExpression newTargetCollection)
        {
            var backupIsInNestedScope = _isInNestedScope;
            _isInNestedScope = true;
            FilterExpression? newFilter = expression.Filter != null ? Visit(expression.Filter, argument) as FilterExpression : null;
            _isInNestedScope = backupIsInNestedScope;

            var newExpression = new HasExpression(newTargetCollection, newFilter);
            return newExpression.Equals(expression) ? expression : newExpression;
        }

        return null;
    }

    public override QueryExpression VisitIsType(IsTypeExpression expression, object? argument)
    {
        ResourceFieldChainExpression? newTargetToOneRelationship = expression.TargetToOneRelationship != null
            ? Visit(expression.TargetToOneRelationship, argument) as ResourceFieldChainExpression
            : null;

        var backupIsInNestedScope = _isInNestedScope;
        _isInNestedScope = true;
        FilterExpression? newChild = expression.Child != null ? Visit(expression.Child, argument) as FilterExpression : null;
        _isInNestedScope = backupIsInNestedScope;

        var newExpression = new IsTypeExpression(newTargetToOneRelationship, expression.DerivedType, newChild);
        return newExpression.Equals(expression) ? expression : newExpression;
    }
}
