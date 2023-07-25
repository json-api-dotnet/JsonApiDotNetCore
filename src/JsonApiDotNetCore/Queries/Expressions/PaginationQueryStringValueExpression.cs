using System.Collections.Immutable;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents pagination in a query string, resulting from text such as:
/// <c>
/// 1,articles:2
/// </c>
/// .
/// </summary>
[PublicAPI]
public class PaginationQueryStringValueExpression : QueryExpression
{
    /// <summary>
    /// The list of one or more pagination elements.
    /// </summary>
    public IImmutableList<PaginationElementQueryStringValueExpression> Elements { get; }

    public PaginationQueryStringValueExpression(IImmutableList<PaginationElementQueryStringValueExpression> elements)
    {
        ArgumentGuard.NotNullNorEmpty(elements);

        Elements = elements;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.PaginationQueryStringValue(this, argument);
    }

    public override string ToString()
    {
        return string.Join(",", Elements.Select(element => element.ToString()));
    }

    public override string ToFullString()
    {
        return string.Join(",", Elements.Select(element => element.ToFullString()));
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

        var other = (PaginationQueryStringValueExpression)obj;

        return Elements.SequenceEqual(other.Elements);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (PaginationElementQueryStringValueExpression element in Elements)
        {
            hashCode.Add(element);
        }

        return hashCode.ToHashCode();
    }
}
