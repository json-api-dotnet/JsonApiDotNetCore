using System.Text;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

/// <summary>
/// Represents the "sum" function, resulting from text such as: sum(orderLines,quantity) or sum(friends,count(children))
/// </summary>
internal sealed class SumExpression : FunctionExpression
{
    public const string Keyword = "sum";

    public ResourceFieldChainExpression TargetToManyRelationship { get; }
    public QueryExpression Selector { get; }

    public override Type ReturnType { get; } = typeof(ulong);

    public SumExpression(ResourceFieldChainExpression targetToManyRelationship, QueryExpression selector)
    {
        ArgumentGuard.NotNull(targetToManyRelationship);
        ArgumentGuard.NotNull(selector);

        TargetToManyRelationship = targetToManyRelationship;
        Selector = selector;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.DefaultVisit(this, argument);
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

        builder.Append(Keyword);
        builder.Append('(');
        builder.Append(toFullString ? TargetToManyRelationship.ToFullString() : TargetToManyRelationship);
        builder.Append(',');
        builder.Append(toFullString ? Selector.ToFullString() : Selector);
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

        var other = (SumExpression)obj;

        return TargetToManyRelationship.Equals(other.TargetToManyRelationship) && Selector.Equals(other.Selector);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TargetToManyRelationship, Selector);
    }
}
