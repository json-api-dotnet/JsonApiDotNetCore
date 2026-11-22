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
    /// The function or attribute whose value to match. Attribute chain format: an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public QueryExpression MatchTarget { get; }

    /// <summary>
    /// The text to match against.
    /// </summary>
    public LiteralConstantExpression Text { get; }

    /// <summary>
    /// The text matching operator to apply.
    /// </summary>
    public TextMatchOperator Operator { get; }

    public MatchTextExpression(QueryExpression matchTarget, LiteralConstantExpression text, TextMatchOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(matchTarget);
        ArgumentNullException.ThrowIfNull(text);

        MatchTarget = matchTarget;
        Text = text;
        Operator = @operator;
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

        builder.Append(Operator.ToString().Camelize());
        builder.Append('(');

        builder.Append(toFullString
            ? string.Join(',', MatchTarget.ToFullString(), Text.ToFullString())
            : string.Join(',', MatchTarget.ToString(), Text.ToString()));

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

        return MatchTarget.Equals(other.MatchTarget) && Text.Equals(other.Text) && Operator == other.Operator;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MatchTarget, Text, Operator);
    }
}
