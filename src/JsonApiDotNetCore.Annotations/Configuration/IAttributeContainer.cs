using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Represents a schema element (such as a resource type or a compound type) that contains JSON:API attributes.
/// </summary>
[PublicAPI]
public interface IAttributeContainer
{
    /// <summary>
    /// The CLR type of the container.
    /// </summary>
    Type ClrType { get; }

    /// <summary>
    /// The attributes contained in this schema element.
    /// </summary>
    IReadOnlyCollection<AttrAttribute> Attributes { get; }

    /// <summary>
    /// Gets the attribute for the specified publicly exposed name. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    AttrAttribute GetAttributeByPublicName(string publicName);

    /// <summary>
    /// Attempts to find an attribute by its publicly exposed name. Returns <c>null</c> when not found.
    /// </summary>
    AttrAttribute? FindAttributeByPublicName(string publicName);

    /// <summary>
    /// Gets the attribute for the specified property name. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    AttrAttribute GetAttributeByPropertyName(string propertyName);

    /// <summary>
    /// Attempts to find an attribute by its property name. Returns <c>null</c> when not found.
    /// </summary>
    AttrAttribute? FindAttributeByPropertyName(string propertyName);
}
