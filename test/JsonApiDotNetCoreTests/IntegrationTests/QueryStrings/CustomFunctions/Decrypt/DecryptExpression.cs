using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

/// <summary>
/// This expression calls the user-defined "decrypt_column_value" database function. It represents the "decrypt" function, resulting from text such as:
/// <c>
/// decrypt(title)
/// </c>
/// , or:
/// <c>
/// decrypt(owner.lastName)
/// </c>
/// , or:
/// <c>
/// decrypt(_)
/// </c>
/// .
/// </summary>
internal sealed class DecryptExpression(QueryExpression target) : FunctionExpression
{
    public const string Keyword = "decrypt";

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="string" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(string);

    /// <summary>
    /// The string attribute or scalar collection element to decrypt.
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

        var other = (DecryptExpression)obj;

        return Target.Equals(other.Target);
    }

    public override int GetHashCode()
    {
        return Target.GetHashCode();
    }
}
