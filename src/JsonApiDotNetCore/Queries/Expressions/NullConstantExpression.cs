using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the constant <c>null</c>, resulting from the text: <c>null</c>.
/// </summary>
[PublicAPI]
public class NullConstantExpression : IdentifierExpression
{
    /// <summary>
    /// Provides access to the singleton instance.
    /// </summary>
    public static readonly NullConstantExpression Instance = new();

    private NullConstantExpression()
    {
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitNullConstant(this, argument);
    }

    public override string ToString()
    {
        return Keywords.Null;
    }

    public override string ToFullString()
    {
        return ToString();
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

        return true;
    }

    public override int GetHashCode()
    {
        return new HashCode().ToHashCode();
    }
}
