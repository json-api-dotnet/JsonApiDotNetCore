using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents a sparse fieldset, resulting from text such as:
/// <c>
/// firstName,lastName,articles
/// </c>
/// .
/// </summary>
[PublicAPI]
public class SparseFieldSetExpression : QueryExpression
{
    /// <summary>
    /// The set of JSON:API fields to include. Chain format: a single field.
    /// </summary>
    public IImmutableSet<ResourceFieldAttribute> Fields { get; }

    public SparseFieldSetExpression(IImmutableSet<ResourceFieldAttribute> fields)
    {
        ArgumentGuard.NotNullNorEmpty(fields);

        Fields = fields;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitSparseFieldSet(this, argument);
    }

    public override string ToString()
    {
        return string.Join(",", Fields.Select(field => field.PublicName).OrderBy(name => name));
    }

    public override string ToFullString()
    {
        return string.Join(".", Fields.Select(field => $"{field.Type.PublicName}:{field.PublicName}").OrderBy(name => name));
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

        var other = (SparseFieldSetExpression)obj;

        return Fields.SetEquals(other.Fields);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (ResourceFieldAttribute field in Fields)
        {
            hashCode.Add(field);
        }

        return hashCode.ToHashCode();
    }
}
