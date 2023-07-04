using System.Text;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

/// <summary>
/// Represents the "length" function, resulting from text such as: length(title)
/// </summary>
internal sealed class LengthExpression : FunctionExpression
{
    public const string Keyword = "length";

    public ResourceFieldChainExpression TargetAttribute { get; }

    public override Type ReturnType { get; } = typeof(int);

    public LengthExpression(ResourceFieldChainExpression targetAttribute)
    {
        ArgumentGuard.NotNull(targetAttribute);

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
        builder.Append(toFullString ? TargetAttribute.ToFullString() : TargetAttribute);
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
