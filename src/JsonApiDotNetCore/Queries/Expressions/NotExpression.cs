using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression tests for the logical negation of its operand. It represents the "not" filter function, resulting from text such as:
/// <c>
/// not(equals(title,'Work'))
/// </c>
/// .
/// </summary>
[PublicAPI]
public class NotExpression : FilterExpression
{
    /// <summary>
    /// The filter whose value to negate.
    /// </summary>
    public FilterExpression Child { get; }

    public NotExpression(FilterExpression child)
    {
        ArgumentNullException.ThrowIfNull(child);

        Child = child;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitNot(this, argument);
    }

    public override string ToString()
    {
        return $"{Keywords.Not}({Child})";
    }

    public override string ToFullString()
    {
        return $"{Keywords.Not}({Child.ToFullString()})";
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

        var other = (NotExpression)obj;

        return Child.Equals(other.Child);
    }

    public override int GetHashCode()
    {
        return Child.GetHashCode();
    }
}
