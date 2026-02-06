using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

/// <summary>
/// This expression tests if the value of a JSON:API attribute is upper case. It represents the "isUpperCase" filter function, resulting from text such
/// as:
/// <c>
/// isUpperCase(title)
/// </c>
/// , or:
/// <c>
/// isUpperCase(owner.lastName)
/// </c>
/// .
/// </summary>
internal sealed class IsUpperCaseExpression : FilterExpression
{
    public const string Keyword = "isUpperCase";

    /// <summary>
    /// The string attribute whose value to inspect. Chain format: an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public ResourceFieldChainExpression TargetAttribute { get; }

    public IsUpperCaseExpression(ResourceFieldChainExpression targetAttribute)
    {
        ArgumentNullException.ThrowIfNull(targetAttribute);

        TargetAttribute = targetAttribute;
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
        builder.Append(toFullString ? TargetAttribute.ToFullString() : TargetAttribute.ToString());
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

        var other = (IsUpperCaseExpression)obj;

        return TargetAttribute.Equals(other.TargetAttribute);
    }

    public override int GetHashCode()
    {
        return TargetAttribute.GetHashCode();
    }
}
