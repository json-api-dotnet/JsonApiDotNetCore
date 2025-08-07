using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents an element in <see cref="PaginationQueryStringValueExpression" />, resulting from text such as: <c>1</c>, or:
/// <c>
/// articles:2
/// </c>
/// .
/// </summary>
[PublicAPI]
public class PaginationElementQueryStringValueExpression(IncludeExpression? scope, int value, int position) : QueryExpression
{
    /// <summary>
    /// The relationship this pagination applies to. Format: zero or more relationships, followed by a to-many relationship.
    /// </summary>
    public IncludeExpression? Scope { get; } = scope;

    /// <summary>
    /// The numeric pagination value.
    /// </summary>
    public int Value { get; } = value;

    /// <summary>
    /// The zero-based position in the text of the query string parameter value.
    /// </summary>
    public int Position { get; } = position;

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitPaginationElementQueryStringValue(this, argument);
    }

    public override string ToString()
    {
        return Scope == null ? $"{Value} at {Position}" : $"{Scope}: {Value} at {Position}";
    }

    public override string ToFullString()
    {
        return Scope == null ? $"{Value} at {Position}" : $"{Scope.ToFullString()}: {Value} at {Position}";
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

        var other = (PaginationElementQueryStringValueExpression)obj;

        return Equals(Scope, other.Scope) && Value == other.Value && Position == other.Position;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Scope, Value, Position);
    }
}
