using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the element value of a primitive collection in a collection filter (such as within <see cref="HasExpression" />).
/// </summary>
[PublicAPI]
public sealed class ElementValueExpression : IdentifierExpression
{
    /// <summary>
    /// The CLR type of the primitive collection element (e.g. <see cref="string" />, <see cref="int" />).
    /// </summary>
    public Type ElementType { get; }

    public ElementValueExpression(Type elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);
        ElementType = elementType;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitElementValue(this, argument);
    }

    public override string ToString() => "_";

    public override string ToFullString() => "_";

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is ElementValueExpression other && ElementType == other.ElementType);

    public override int GetHashCode() => ElementType.GetHashCode();
}
