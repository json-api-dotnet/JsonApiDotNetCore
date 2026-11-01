using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the element value of a scalar collection (_), resulting from text such as:
/// <c>
/// has(tags,equals(_,'Work'))
/// </c>
/// .
/// </summary>
[PublicAPI]
public sealed class ScalarCollectionElementValueExpression : IdentifierExpression
{
    private const string Symbol = "_";

    /// <summary>
    /// The CLR type of the scalar collection element, such as <see cref="string" /> or <see cref="int" />.
    /// </summary>
    public Type ElementType { get; }

    public ScalarCollectionElementValueExpression(Type elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        ElementType = elementType;
    }

    /// <inheritdoc />
    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitScalarCollectionElementValue(this, argument);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Symbol;
    }

    /// <inheritdoc />
    public override string ToFullString()
    {
        return ToString();
    }

    /// <inheritdoc />
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

        var other = (ScalarCollectionElementValueExpression)obj;

        return ElementType == other.ElementType;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return ElementType.GetHashCode();
    }
}
