using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression allows to test if a resource in an inheritance hierarchy can be upcast to a derived type, optionally with a condition where the
/// derived type is accessible. It represents the "isType" filter function, resulting from text such as:
/// <c>
/// isType(,men)
/// </c>
/// ,
/// <c>
/// isType(creator,men)
/// </c>
/// , or:
/// <c>
/// isType(creator,men,equals(hasBeard,'true'))
/// </c>
/// .
/// </summary>
[PublicAPI]
public class IsTypeExpression : FilterExpression
{
    /// <summary>
    /// An optional to-one relationship to start from. Chain format: one or more to-one relationships.
    /// </summary>
    public ResourceFieldChainExpression? TargetToOneRelationship { get; }

    /// <summary>
    /// The derived resource type to upcast to.
    /// </summary>
    public ResourceType DerivedType { get; }

    /// <summary>
    /// An optional filter that the derived resource must match.
    /// </summary>
    public FilterExpression? Child { get; }

    public IsTypeExpression(ResourceFieldChainExpression? targetToOneRelationship, ResourceType derivedType, FilterExpression? child)
    {
        ArgumentNullException.ThrowIfNull(derivedType);

        TargetToOneRelationship = targetToOneRelationship;
        DerivedType = derivedType;
        Child = child;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitIsType(this, argument);
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
        builder.Append(Keywords.IsType);
        builder.Append('(');

        if (TargetToOneRelationship != null)
        {
            builder.Append(toFullString ? TargetToOneRelationship.ToFullString() : TargetToOneRelationship.ToString());
        }

        builder.Append(',');
        builder.Append(DerivedType);

        if (Child != null)
        {
            builder.Append(',');
            builder.Append(toFullString ? Child.ToFullString() : Child.ToString());
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

        var other = (IsTypeExpression)obj;

        return Equals(TargetToOneRelationship, other.TargetToOneRelationship) && DerivedType.Equals(other.DerivedType) && Equals(Child, other.Child);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TargetToOneRelationship, DerivedType, Child);
    }
}
