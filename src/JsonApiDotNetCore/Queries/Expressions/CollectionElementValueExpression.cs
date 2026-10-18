using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Represents the element value of a collection (such as a scalar collection) in an expression tree.
/// </summary>
[PublicAPI]
public sealed class CollectionElementValueExpression : IdentifierExpression
{
    internal const string Symbol = "_";

    /// <summary>
    /// The CLR type of the collection element (e.g. <see cref="string" />, <see cref="int" />).
    /// </summary>
    public Type ElementType { get; }

    public CollectionElementValueExpression(Type elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);
        ElementType = elementType;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitCollectionElementValue(this, argument);
    }

    public override string ToString()
    {
        return Symbol;
    }

    public override string ToFullString()
    {
        return ToString();
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is CollectionElementValueExpression other && ElementType == other.ElementType);
    }

    public override int GetHashCode()
    {
        return ElementType.GetHashCode();
    }
}
