using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Response;

/// <summary>
/// Builds the top-level meta object.
/// </summary>
[PublicAPI]
public interface IMetaBuilder
{
    /// <summary>
    /// Merges the specified dictionary with existing key/value pairs. In the event of a key collision, the value from the specified dictionary will
    /// overwrite the existing one.
    /// </summary>
    void Add(IDictionary<string, object?> values);

    /// <summary>
    /// Builds the top-level meta data object.
    /// </summary>
#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    IDictionary<string, object?>? Build();
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
}
