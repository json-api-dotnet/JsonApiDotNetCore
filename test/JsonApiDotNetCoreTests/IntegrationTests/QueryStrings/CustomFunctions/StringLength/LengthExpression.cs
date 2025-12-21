using System.Text;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

/// <summary>
/// This expression allows to determine the string length of a JSON:API attribute. It represents the "length" function, resulting from text such as:
/// <c>
/// length(title)
/// </c>
/// , or:
/// <c>
/// length(owner.lastName)
/// </c>
/// .
/// </summary>
internal sealed class LengthExpression : FunctionExpression
{
    public const string Keyword = "length";

    /// <summary>
    /// The string attribute whose length to determine. Chain format: an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public ResourceFieldChainExpression TargetAttribute { get; }

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="int" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(int);

    public LengthExpression(ResourceFieldChainExpression targetAttribute)
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

        var other = (LengthExpression)obj;

        return TargetAttribute.Equals(other.TargetAttribute);
    }

    public override int GetHashCode()
    {
        return TargetAttribute.GetHashCode();
    }
}
