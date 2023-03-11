using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents a non-null constant value, resulting from text such as: equals(firstName,'Jack')
/// </summary>
[PublicAPI]
public class LiteralConstantExpression : IdentifierExpression
{
    private readonly string _stringValue;

    public object TypedValue { get; }

    public LiteralConstantExpression(object typedValue)
        : this(typedValue, typedValue.ToString()!)
    {
    }

    public LiteralConstantExpression(object typedValue, string stringValue)
    {
        ArgumentGuard.NotNull(typedValue);
        ArgumentGuard.NotNull(stringValue);

        TypedValue = typedValue;
        _stringValue = stringValue;
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

        return Equals(TypedValue, other.TypedValue) && _stringValue == other._stringValue;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TypedValue, _stringValue);
    }
}
