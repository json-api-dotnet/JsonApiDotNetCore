using System.Text;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents an element in <see cref="SortExpression" />.
/// </summary>
[PublicAPI]
public class SortElementExpression : QueryExpression
{
    public QueryExpression Target { get; }
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
