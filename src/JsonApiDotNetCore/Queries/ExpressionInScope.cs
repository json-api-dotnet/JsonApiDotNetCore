using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// The expression that represents a parsed query string parameter, along with the scope to which it applies.
/// </summary>
public abstract class ExpressionInScope
{
    public QueryExpression Expression { get; }

    /// <summary>
    /// This constructor prevents external inheritance.
    /// </summary>
    private protected ExpressionInScope(QueryExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        Expression = expression;
    }
}
