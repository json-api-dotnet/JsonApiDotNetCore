using System.Text;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents an element in <see cref="SortExpression" />, resulting from text such as: <c>lastName</c>,
/// <c>
/// -lastModifiedAt
/// </c>
/// , or:
/// <c>
/// count(children)
/// </c>
/// .
/// </summary>
[PublicAPI]
public class SortElementExpression : QueryExpression
{
    /// <summary>
    /// The target to sort on, which can be a function or a field chain. Chain format: an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public QueryExpression Target { get; }

    /// <summary>
    /// Indicates the sort direction.
    /// </summary>
    public bool IsAscending { get; }

    public SortElementExpression(QueryExpression target, bool isAscending)
    {
        ArgumentGuard.NotNull(target);

        Target = target;
        IsAscending = isAscending;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitSortElement(this, argument);
    }

    public override string ToString()
    {
        return InnerToString(false);
    }

    public override string ToFullString()
    {
        return InnerToString(true);
    }

    private string InnerToString(bool toFullString)
    {
        var builder = new StringBuilder();

        if (!IsAscending)
        {
            builder.Append('-');
        }

        builder.Append(toFullString ? Target.ToFullString() : Target);

        return builder.ToString();
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

        var other = (SortElementExpression)obj;

        return Equals(Target, other.Target) && IsAscending == other.IsAscending;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Target, IsAscending);
    }
}
