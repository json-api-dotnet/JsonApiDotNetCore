using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the relationship scope of a query string parameter, resulting from text such as:
/// <c>
/// ?sort[articles]
/// </c>
/// , or:
/// <c>
/// ?filter[author.articles.comments]
/// </c>
/// .
/// </summary>
[PublicAPI]
public class QueryStringParameterScopeExpression : QueryExpression
{
    /// <summary>
    /// The name of the query string parameter, without its surrounding brackets.
    /// </summary>
    public LiteralConstantExpression ParameterName { get; }

    /// <summary>
    /// The scope this parameter value applies to, or <c>null</c> for the URL endpoint scope. Chain format for the filter/sort parameters: an optional list
    /// of relationships, followed by a to-many relationship.
    /// </summary>
    public IncludeExpression? Scope { get; }

    public QueryStringParameterScopeExpression(LiteralConstantExpression parameterName, IncludeExpression? scope)
    {
        ArgumentNullException.ThrowIfNull(parameterName);

        ParameterName = parameterName;
        Scope = scope;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitQueryStringParameterScope(this, argument);
    }

    public override string ToString()
    {
        return Scope == null ? ParameterName.ToString() : $"{ParameterName}: {Scope}";
    }

    public override string ToFullString()
    {
        return Scope == null ? ParameterName.ToFullString() : $"{ParameterName.ToFullString()}: {Scope.ToFullString()}";
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (QueryStringParameterScopeExpression)obj;

        return ParameterName.Equals(other.ParameterName) && Equals(Scope, other.Scope);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ParameterName, Scope);
    }
}
