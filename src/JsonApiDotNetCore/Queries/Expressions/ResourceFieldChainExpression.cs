using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents a chain of JSON:API fields (relationships and attributes), resulting from text such as:
/// <c>
/// articles.revisions.author
/// </c>
/// , or:
/// <c>
/// owner.LastName
/// </c>
/// .
/// </summary>
[PublicAPI]
public class ResourceFieldChainExpression : IdentifierExpression
{
    /// <summary>
    /// A list of one or more JSON:API fields. Use <see cref="FieldChainPattern.Match" /> to convert from text.
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> Fields { get; }

    public ResourceFieldChainExpression(ResourceFieldAttribute field)
    {
        ArgumentGuard.NotNull(field);

        Fields = ImmutableArray.Create(field);
    }

    public ResourceFieldChainExpression(IImmutableList<ResourceFieldAttribute> fields)
    {
        ArgumentGuard.NotNullNorEmpty(fields);

        Fields = fields;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitResourceFieldChain(this, argument);
    }

    public override string ToString()
    {
        return string.Join(".", Fields.Select(field => field.PublicName));
    }

    public override string ToFullString()
    {
        return string.Join(".", Fields.Select(field => $"{field.Type.PublicName}:{field.PublicName}"));
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

        var other = (ResourceFieldChainExpression)obj;

        return Fields.SequenceEqual(other.Fields);
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
