using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression determines the number of related resources in a to-many relationship. It represents the "count" function, resulting from text such
/// as:
/// <c>
/// count(articles)
/// </c>
/// .
/// </summary>
[PublicAPI]
public class CountExpression : FunctionExpression
{
    /// <summary>
    /// The to-many relationship to count related resources for. Chain format: an optional list of to-one relationships, followed by a to-many relationship.
    /// </summary>
    public ResourceFieldChainExpression TargetCollection { get; }

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="int" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(int);

    public CountExpression(ResourceFieldChainExpression targetCollection)
    {
        ArgumentNullException.ThrowIfNull(targetCollection);

        TargetCollection = targetCollection;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitCount(this, argument);
    }

    public override string ToString()
    {
        return $"{Keywords.Count}({TargetCollection})";
    }

    public override string ToFullString()
    {
        return $"{Keywords.Count}({TargetCollection.ToFullString()})";
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

        var other = (CountExpression)obj;

        return TargetCollection.Equals(other.TargetCollection);
    }

    public override int GetHashCode()
    {
        return TargetCollection.GetHashCode();
    }
}
