using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

/// <summary>
/// This expression determines the string length of a JSON:API attribute or scalar collection element. It represents the "length" function, resulting
/// from text such as:
/// <c>
/// length(title)
/// </c>
/// , or:
/// <c>
/// length(owner.lastName)
/// </c>
/// , or:
/// <c>
/// length(_)
/// </c>
/// .
/// </summary>
internal sealed class LengthExpression(QueryExpression target) : FunctionExpression
{
    public const string Keyword = "length";

    /// <summary>
    /// The string attribute or scalar collection element whose length to determine.
    /// </summary>
    public QueryExpression Target { get; } = target;

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="int" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(int);

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

        var other = (LengthExpression)obj;

        return Target.Equals(other.Target);
    }

    public override int GetHashCode()
    {
        return Target.GetHashCode();
    }
}
