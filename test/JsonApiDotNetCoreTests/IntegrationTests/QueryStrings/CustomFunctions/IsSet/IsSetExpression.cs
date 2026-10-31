using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsSet;

/// <summary>
/// This expression tests if a JSON:API attribute of any kind (including compound attributes) has a value. It represents the "isSet" filter function,
/// resulting from text such as:
/// <c>
/// isSet(location)
/// </c>
/// , or:
/// <c>
/// isSet(location.address)
/// </c>
/// .
/// </summary>
internal sealed class IsSetExpression(ResourceFieldChainExpression target) : FilterExpression
{
    public const string Keyword = "isSet";

    /// <summary>
    /// The attribute whose value to inspect.
    /// </summary>
    public ResourceFieldChainExpression Target { get; } = target;

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.DefaultVisit(this, argument);
    }

    public override string ToString()
    {
        return $"{Keyword}({Target})";
    }

    public override string ToFullString()
    {
        return $"{Keyword}({Target.ToFullString()})";
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

        var other = (IsSetExpression)obj;

        return Target.Equals(other.Target);
    }

    public override int GetHashCode()
    {
        return Target.GetHashCode();
    }
}
