using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// The expression that represents a parsed <c>filter</c>, <c>sort</c>, <c>page</c>, <c>include</c> or handler query string parameter, along with the
/// relationship chain to which it applies. A <c>null</c> chain indicates the request's resource scope.
/// </summary>
[PublicAPI]
public sealed class RelationshipChainExpressionInScope(ResourceFieldChainExpression? scope, QueryExpression expression)
    : ExpressionInScope(expression)
{
    public ResourceFieldChainExpression? Scope { get; } = scope;

    public override string ToString()
    {
        return $"{Scope} => {Expression}";
    }
}
