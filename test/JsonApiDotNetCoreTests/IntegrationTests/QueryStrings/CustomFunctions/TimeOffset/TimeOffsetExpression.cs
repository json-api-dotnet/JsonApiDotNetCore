using System.Text;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

/// <summary>
/// This expression wraps a time duration. It represents the "timeOffset" function, resulting from text such as:
/// <c>
/// timeOffset('+0:10:00')
/// </c>
/// , or:
/// <c>
/// timeOffset('-0:10:00')
/// </c>
/// .
/// </summary>
internal sealed class TimeOffsetExpression : FunctionExpression
{
    public const string Keyword = "timeOffset";

    // Only used to show the original input in errors and diagnostics. Not part of the semantic expression value.
    private readonly LiteralConstantExpression _timeSpanConstant;

    /// <summary>
    /// The time offset, which can be negative.
    /// </summary>
    public TimeSpan Value { get; }

    /// <summary>
    /// The CLR type this function returns, which is always <see cref="TimeSpan" />.
    /// </summary>
    public override Type ReturnType { get; } = typeof(TimeSpan);

    public TimeOffsetExpression(LiteralConstantExpression timeSpanConstant)
    {
        ArgumentGuard.NotNull(timeSpanConstant);

        if (timeSpanConstant.TypedValue is not TimeSpan timeSpan)
        {
            throw new ArgumentException($"Constant must contain a {nameof(TimeSpan)}.", nameof(timeSpanConstant));
        }

        _timeSpanConstant = timeSpanConstant;

        Value = timeSpan;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.DefaultVisit(this, argument);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append(Keyword);
        builder.Append('(');
        builder.Append(_timeSpanConstant);
        builder.Append(')');

        return builder.ToString();
    }

    public override string ToFullString()
    {
        return ToString();
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

        var other = (TimeOffsetExpression)obj;

        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
