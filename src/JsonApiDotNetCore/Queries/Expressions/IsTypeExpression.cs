using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the "isType" filter function, resulting from text such as: isType(,men), isType(creator,men) or
/// isType(creator,men,equals(hasBeard,'true'))
/// </summary>
[PublicAPI]
public class IsTypeExpression : FilterExpression
{
    public ResourceFieldChainExpression? TargetToOneRelationship { get; }
    public ResourceType DerivedType { get; }
    public FilterExpression? Child { get; }

    public IsTypeExpression(ResourceFieldChainExpression? targetToOneRelationship, ResourceType derivedType, FilterExpression? child)
    {
        ArgumentGuard.NotNull(derivedType, nameof(derivedType));

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
        var builder = new StringBuilder();
        builder.Append(Keywords.IsType);
        builder.Append('(');

        if (TargetToOneRelationship != null)
        {
            builder.Append(TargetToOneRelationship);
        }

        builder.Append(',');
        builder.Append(DerivedType);

        if (Child != null)
        {
            builder.Append(',');
            builder.Append(Child);
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
