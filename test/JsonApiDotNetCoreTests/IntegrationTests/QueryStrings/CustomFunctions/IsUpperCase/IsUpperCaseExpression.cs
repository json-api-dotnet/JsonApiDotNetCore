using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

/// <summary>
/// This expression tests if the value of a JSON:API attribute or scalar collection element is upper case. It represents the "isUpperCase" filter
/// function, resulting from text such as:
/// <c>
/// isUpperCase(title)
/// </c>
/// , or:
/// <c>
/// isUpperCase(owner.lastName)
/// </c>
/// , or:
/// <c>
/// isUpperCase(_)
/// </c>
/// .
/// </summary>
internal sealed class IsUpperCaseExpression(QueryExpression target) : FilterExpression
{
    public const string Keyword = "isUpperCase";

    /// <summary>
    /// The string attribute or scalar collection element whose value to inspect.
    /// </summary>
    public QueryExpression Target { get; } = target;

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
        builder.Append(toFullString ? Target.ToFullString() : Target.ToString());
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

        return Target.Equals(other.Target);
    }

    public override int GetHashCode()
    {
        return Target.GetHashCode();
    }
}
