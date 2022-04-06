using System.Collections.Immutable;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents an inclusion tree, resulting from text such as: owner,articles.revisions
/// </summary>
[PublicAPI]
public class IncludeExpression : QueryExpression
{
    private static readonly IncludeChainConverter IncludeChainConverter = new();

    public static readonly IncludeExpression Empty = new();

    public IImmutableSet<IncludeElementExpression> Elements { get; }

    public IncludeExpression(IImmutableSet<IncludeElementExpression> elements)
    {
        ArgumentGuard.NotNullNorEmpty(elements, nameof(elements));

        Elements = elements;
    }

    private IncludeExpression()
    {
        Elements = ImmutableHashSet<IncludeElementExpression>.Empty;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitInclude(this, argument);
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
        IReadOnlyCollection<ResourceFieldChainExpression> chains = IncludeChainConverter.GetRelationshipChains(this);
        return string.Join(",", chains.Select(field => toFullString ? field.ToFullString() : field.ToString()).OrderBy(name => name));
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

        var other = (IncludeExpression)obj;

        return Elements.SetEquals(other.Elements);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (IncludeElementExpression element in Elements)
        {
            hashCode.Add(element);
        }

        return hashCode.ToHashCode();
    }
}
