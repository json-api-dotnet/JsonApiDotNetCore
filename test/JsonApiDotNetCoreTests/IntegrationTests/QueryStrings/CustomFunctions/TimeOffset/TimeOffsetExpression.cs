using System.Text;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

/// <summary>
/// Represents the "timeOffset" function, resulting from text such as: timeOffset('-0:10:00')
/// </summary>
internal sealed class TimeOffsetExpression : FunctionExpression
{
    public const string Keyword = "timeOffset";

    // Only used to show the original input in errors and diagnostics. Not part of the semantic expression value.
    private readonly LiteralConstantExpression _timeSpanConstant;

    public TimeSpan Value { get; }

    public override Type ReturnType { get; } = typeof(TimeSpan);

    public TimeOffsetExpression(LiteralConstantExpression timeSpanConstant)
    {
        ArgumentGuard.NotNull(timeSpanConstant);

        if (timeSpanConstant.TypedValue.GetType() != typeof(TimeSpan))
        {
            throw new ArgumentException($"Constant must contain a {nameof(TimeSpan)}.", nameof(timeSpanConstant));
        }

        _timeSpanConstant = timeSpanConstant;

        Value = (TimeSpan)timeSpanConstant.TypedValue;
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
