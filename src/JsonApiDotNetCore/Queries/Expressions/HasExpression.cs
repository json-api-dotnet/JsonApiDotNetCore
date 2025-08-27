using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression allows to test if a to-many relationship has related resources, optionally with a condition. It represents the "has" filter function,
/// resulting from text such as:
/// <c>
/// has(articles)
/// </c>
/// , or:
/// <c>
/// has(articles,equals(isHidden,'false'))
/// </c>
/// .
/// </summary>
[PublicAPI]
public class HasExpression : FilterExpression
{
    /// <summary>
    /// The to-many relationship to determine related resources for. Chain format: an optional list of to-one relationships, followed by a to-many
    /// relationship.
    /// </summary>
    public ResourceFieldChainExpression TargetCollection { get; }

    /// <summary>
    /// An optional filter that is applied on the related resources. Any related resources that do not match the filter are ignored.
    /// </summary>
    public FilterExpression? Filter { get; }

    public HasExpression(ResourceFieldChainExpression targetCollection, FilterExpression? filter)
    {
        ArgumentNullException.ThrowIfNull(targetCollection);

        TargetCollection = targetCollection;
        Filter = filter;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitHas(this, argument);
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
        builder.Append(Keywords.Has);
        builder.Append('(');
        builder.Append(toFullString ? TargetCollection.ToFullString() : TargetCollection.ToString());

        if (Filter != null)
        {
            builder.Append(',');
            builder.Append(toFullString ? Filter.ToFullString() : Filter.ToString());
        }

        builder.Append(')');

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

        var other = (HasExpression)obj;

        return TargetCollection.Equals(other.TargetCollection) && Equals(Filter, other.Filter);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TargetCollection, Filter);
    }
}
