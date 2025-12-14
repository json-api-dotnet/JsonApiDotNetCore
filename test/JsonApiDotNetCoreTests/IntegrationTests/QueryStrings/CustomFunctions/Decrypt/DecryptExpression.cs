using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

/// <summary>
/// This expression allows to call the user-defined "decrypt_column_value" database function. It represents the "decrypt" function, resulting from text
/// such as:
/// <c>
/// decrypt(title)
/// </c>
/// , or:
/// <c>
/// decrypt(owner.lastName)
/// </c>
/// .
/// </summary>
internal sealed class DecryptExpression(ResourceFieldChainExpression targetAttribute) : FunctionExpression
{
    public const string Keyword = "decrypt";

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="string" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(string);

    /// <summary>
    /// The string attribute to decrypt. Chain format: an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public ResourceFieldChainExpression TargetAttribute { get; } = targetAttribute;

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

        var other = (DecryptExpression)obj;

        return TargetAttribute.Equals(other.TargetAttribute);
    }

    public override int GetHashCode()
    {
        return TargetAttribute.GetHashCode();
    }
}
