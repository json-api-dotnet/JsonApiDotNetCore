using System.Text;
using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression allows partial matching on the value of a JSON:API attribute. It represents text-matching filter functions, resulting from text such
/// as:
/// <c>
/// startsWith(name,'The')
/// </c>
/// ,
/// <c>
/// endsWith(name,'end.')
/// </c>
/// , or:
/// <c>
/// contains(name,'middle')
/// </c>
/// .
/// </summary>
[PublicAPI]
public class MatchTextExpression : FilterExpression
{
    /// <summary>
    /// The attribute whose value to match. Chain format: an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public ResourceFieldChainExpression TargetAttribute { get; }

    /// <summary>
    /// The text to match the attribute's value against.
    /// </summary>
    public LiteralConstantExpression TextValue { get; }

    /// <summary>
    /// The kind of matching to perform.
    /// </summary>
    public TextMatchKind MatchKind { get; }

    public MatchTextExpression(ResourceFieldChainExpression targetAttribute, LiteralConstantExpression textValue, TextMatchKind matchKind)
    {
        ArgumentNullException.ThrowIfNull(targetAttribute);
        ArgumentNullException.ThrowIfNull(textValue);

        TargetAttribute = targetAttribute;
        TextValue = textValue;
        MatchKind = matchKind;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitMatchText(this, argument);
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

        builder.Append(MatchKind.ToString().Camelize());
        builder.Append('(');

        builder.Append(toFullString
            ? string.Join(',', TargetAttribute.ToFullString(), TextValue.ToFullString())
            : string.Join(',', TargetAttribute.ToString(), TextValue.ToString()));

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

        var other = (MatchTextExpression)obj;

        return TargetAttribute.Equals(other.TargetAttribute) && TextValue.Equals(other.TextValue) && MatchKind == other.MatchKind;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TargetAttribute, TextValue, MatchKind);
    }
}
