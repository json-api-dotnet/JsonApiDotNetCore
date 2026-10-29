using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents a sparse fieldset, resulting from text such as:
/// <c>
/// firstName,lastName,articles
/// </c>
/// , or:
/// <c>
/// contact.name.firstName,contact.address
/// </c>
/// .
/// </summary>
[PublicAPI]
public class SparseFieldSetExpression : QueryExpression
{
    /// <summary>
    /// Indicates not to include any fields.
    /// </summary>
    public static readonly SparseFieldSetExpression Empty = new();

    /// <summary>
    /// The JSON:API fields included in this sparse fieldset tree, keyed by field (attribute or relationship).
    /// </summary>
    public IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> Elements { get; }

    public SparseFieldSetExpression(IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> elements)
    {
        ArgumentGuard.NotNullNorEmpty(elements);

        Elements = elements;
    }

    private SparseFieldSetExpression()
    {
        Elements = ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty;
    }

    internal static SparseFieldSetExpression Create(IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);

        return elements.Count > 0 ? new SparseFieldSetExpression(elements) : Empty;
    }

    /// <inheritdoc />
    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitSparseFieldSet(this, argument);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return InnerToString(false);
    }

    /// <inheritdoc />
    public override string ToFullString()
    {
        return InnerToString(true);
    }

    private string InnerToString(bool toFullString)
    {
        return string.Join(',', Elements.Values.SelectMany(element => SparseFieldSetElementExpression.Flatten(element, toFullString)).Order());
    }

    /// <inheritdoc />
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

        var other = (SparseFieldSetExpression)obj;

        return Elements.DictionaryEquals(other.Elements);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach ((ResourceFieldAttribute field, SparseFieldSetElementExpression element) in Elements.OrderBy(pair => pair.Key.PublicName))
        {
            hashCode.Add(field);
            hashCode.Add(element);
        }

        return hashCode.ToHashCode();
    }
}
