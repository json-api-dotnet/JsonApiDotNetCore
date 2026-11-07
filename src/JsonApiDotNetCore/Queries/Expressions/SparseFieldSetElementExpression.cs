using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents an element in a <see cref="SparseFieldSetExpression" /> tree, resulting from text such as:
/// <c>
/// contact.name.firstName,contact.name.lastName
/// </c>
/// .
/// </summary>
[PublicAPI]
public class SparseFieldSetElementExpression : QueryExpression
{
    private static readonly ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> EmptyChildren =
        ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Empty;

    /// <summary>
    /// The JSON:API field (attribute or relationship) to include.
    /// </summary>
    public ResourceFieldAttribute Field { get; }

    /// <summary>
    /// The direct children of this subtree for compound attributes, keyed by child field. An empty dictionary indicates that all viewable child attributes
    /// are included.
    /// </summary>
    public IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> Children { get; }

    public SparseFieldSetElementExpression(ResourceFieldAttribute field)
        : this(field, EmptyChildren)
    {
    }

    public SparseFieldSetElementExpression(ResourceFieldAttribute field, IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> children)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(children);

        Field = field;
        Children = children;
    }

    /// <inheritdoc />
    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitSparseFieldSetElement(this, argument);
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
        return string.Join(',', Flatten(this, toFullString).Order());
    }

    internal static IEnumerable<string> Flatten(SparseFieldSetElementExpression element, bool toFullString)
    {
        string fieldText = toFullString ? element.Field.ToFullString() : element.Field.ToString()!;

        if (element.Children.Count == 0)
        {
            yield return fieldText;
        }
        else
        {
            foreach (SparseFieldSetElementExpression child in element.Children.Values)
            {
                foreach (string childText in Flatten(child, toFullString))
                {
                    yield return $"{fieldText}.{childText}";
                }
            }
        }
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

        var other = (SparseFieldSetElementExpression)obj;

        return Field.Equals(other.Field) && Children.DictionaryEquals(other.Children);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Field);

        foreach ((ResourceFieldAttribute childField, SparseFieldSetElementExpression childElement) in Children.OrderBy(pair => pair.Key.PublicName))
        {
            hashCode.Add(childField);
            hashCode.Add(childElement);
        }

        return hashCode.ToHashCode();
    }
}
