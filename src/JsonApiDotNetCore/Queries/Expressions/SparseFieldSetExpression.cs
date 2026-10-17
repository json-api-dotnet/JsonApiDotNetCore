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
    /// The set of JSON:API fields to include.
    /// </summary>
    public IImmutableSet<ResourceFieldAttribute> Fields { get; }

    /// <summary>
    /// The set of JSON:API field chains to include.
    /// </summary>
    public IImmutableSet<ResourceFieldChainExpression> FieldChains { get; }

    public SparseFieldSetExpression(IImmutableSet<ResourceFieldAttribute> fields)
    {
        ArgumentGuard.NotNullNorEmpty(fields);

        Fields = fields;
        FieldChains = fields.Select(field => new ResourceFieldChainExpression(ImmutableArray.Create(field))).ToImmutableHashSet();
    }

    public SparseFieldSetExpression(IImmutableSet<ResourceFieldChainExpression> fieldChains)
    {
        ArgumentGuard.NotNullNorEmpty(fieldChains);

        FieldChains = fieldChains;
        Fields = fieldChains.Select(chain => chain.Fields[0]).ToImmutableHashSet();
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitSparseFieldSet(this, argument);
    }

    public override string ToString()
    {
        return string.Join(',', FieldChains.Select(chain => chain.ToString()).Order());
    }

    public override string ToFullString()
    {
        return string.Join(',', FieldChains.Select(chain => $"{chain.ToFullString()}").Order());
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

        return FieldChains.SetEquals(other.FieldChains);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (ResourceFieldChainExpression chain in FieldChains)
        {
            hashCode.Add(chain);
        }

        return hashCode.ToHashCode();
    }
}
