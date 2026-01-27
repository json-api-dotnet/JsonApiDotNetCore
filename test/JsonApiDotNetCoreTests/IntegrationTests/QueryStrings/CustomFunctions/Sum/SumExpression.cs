using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

/// <summary>
/// This expression allows to determine the sum of values in the related resources of a to-many relationship. It represents the "sum" function, resulting
/// from text such as:
/// <c>
/// sum(orderLines,quantity)
/// </c>
/// , or:
/// <c>
/// sum(friends,count(children))
/// </c>
/// .
/// </summary>
internal sealed class SumExpression : FunctionExpression
{
    public const string Keyword = "sum";

    /// <summary>
    /// The to-many relationship whose related resources are summed over.
    /// </summary>
    public ResourceFieldChainExpression TargetToManyRelationship { get; }

    /// <summary>
    /// The selector to apply on related resources, which can be a function or a field chain. Chain format: an optional list of to-one relationships,
    /// followed by an attribute. The selector must return a numeric type.
    /// </summary>
    public QueryExpression Selector { get; }

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="ulong" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(ulong);

    public SumExpression(ResourceFieldChainExpression targetToManyRelationship, QueryExpression selector)
    {
        ArgumentNullException.ThrowIfNull(targetToManyRelationship);
        ArgumentNullException.ThrowIfNull(selector);

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
        builder.Append(toFullString ? TargetToManyRelationship.ToFullString() : TargetToManyRelationship.ToString());
        builder.Append(',');
        builder.Append(toFullString ? Selector.ToFullString() : Selector.ToString());
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
