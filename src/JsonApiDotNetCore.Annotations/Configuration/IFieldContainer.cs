using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// The containing type for a JSON:API field, which can be a <see cref="ResourceType" /> or an <see cref="AttrAttribute" />.
/// </summary>
public interface IFieldContainer
{
    /// <summary>
    /// The publicly exposed name of this container.
    /// </summary>
    string PublicName { get; }

    /// <summary>
    /// The CLR type of this container.
    /// </summary>
    Type ClrType { get; }

    /// <summary>
    /// Searches the direct children of this container for an attribute with the specified name.
    /// </summary>
    /// <param name="publicName">
    /// The publicly exposed name of the attribute to find.
    /// </param>
    /// <returns>
    /// The attribute, or <c>null</c> if not found.
    /// </returns>
    AttrAttribute? FindAttributeByPublicName(string publicName);
}
