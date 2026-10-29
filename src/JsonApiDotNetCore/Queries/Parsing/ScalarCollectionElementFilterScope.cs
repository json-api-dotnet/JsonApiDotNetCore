using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Represents a filter scope on the element of a scalar collection attribute.
/// </summary>
[PublicAPI]
public sealed class ScalarCollectionElementFilterScope : FilterScope
{
    /// <summary>
    /// Gets the CLR type of the scalar collection element.
    /// </summary>
    public Type ElementType { get; }

    public ScalarCollectionElementFilterScope(Type elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        ElementType = elementType;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ElementType.Name;
    }
}
