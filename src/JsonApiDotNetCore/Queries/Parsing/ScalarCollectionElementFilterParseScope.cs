using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Represents a filter scope on the element of a scalar collection attribute.
/// </summary>
[PublicAPI]
public sealed class ScalarCollectionElementFilterParseScope : FilterParseScope
{
    /// <summary>
    /// Gets the CLR type of the scalar collection element.
    /// </summary>
    public Type ElementType { get; }

    public ScalarCollectionElementFilterParseScope(Type elementType)
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
