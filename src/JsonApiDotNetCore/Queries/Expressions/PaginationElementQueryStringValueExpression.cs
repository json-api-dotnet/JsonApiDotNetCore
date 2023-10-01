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
public class PaginationElementQueryStringValueExpression : QueryExpression
{
    /// <summary>
    /// The relationship this pagination applies to. Chain format: zero or more relationships, followed by a to-many relationship.
    /// </summary>
    public ResourceFieldChainExpression? Scope { get; }

    /// <summary>
    /// The numeric pagination value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// The zero-based position in the text of the query string parameter value.
    /// </summary>
    public int Position { get; }

    public PaginationElementQueryStringValueExpression(ResourceFieldChainExpression? scope, int value, int position)
    {
        Scope = scope;
        Value = value;
        Position = position;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.PaginationElementQueryStringValue(this, argument);
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
