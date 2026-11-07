using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// The expression that represents a parsed <c>fields</c> query string parameter, along with the resource type to which it applies.
/// </summary>
[PublicAPI]
public sealed class ResourceTypeExpressionInScope : ExpressionInScope
{
    public ResourceType ResourceType { get; }

    public ResourceTypeExpressionInScope(ResourceType resourceType, QueryExpression expression)
        : base(expression)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        ResourceType = resourceType;
    }

    public override string ToString()
    {
        return $"{ResourceType} => {Expression}";
    }
}
