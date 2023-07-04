using System.Globalization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents a non-null constant value, resulting from text such as: equals(firstName,'Jack')
/// </summary>
[PublicAPI]
public class LiteralConstantExpression : IdentifierExpression
{
    // Only used to show the original input in errors and diagnostics. Not part of the semantic expression value.
    private readonly string _stringValue;

    public object TypedValue { get; }

    public LiteralConstantExpression(object typedValue)
        : this(typedValue, GetStringValue(typedValue)!)
    {
    }

    public LiteralConstantExpression(object typedValue, string stringValue)
    {
        ArgumentGuard.NotNull(typedValue);
        ArgumentGuard.NotNull(stringValue);

        TypedValue = typedValue;
        _stringValue = stringValue;
    }

    private static string? GetStringValue(object typedValue)
    {
        ArgumentGuard.NotNull(typedValue);

        return typedValue is IFormattable cultureAwareValue ? cultureAwareValue.ToString(null, CultureInfo.InvariantCulture) : typedValue.ToString();
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitLiteralConstant(this, argument);
    }

    public override string ToString()
    {
        string escapedValue = _stringValue.Replace("\'", "\'\'");
        return $"'{escapedValue}'";
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

        var other = (LiteralConstantExpression)obj;

        return TypedValue.Equals(other.TypedValue);
    }

    public override int GetHashCode()
    {
        return TypedValue.GetHashCode();
    }
}
