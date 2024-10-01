using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Validates and converts a <see cref="Document" /> belonging to an atomic:operations request.
/// </summary>
public interface IDocumentInOperationsRequestAdapter
{
    /// <summary>
    /// Validates and converts the specified <paramref name="document" />.
    /// </summary>
#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    IList<OperationContainer> Convert(Document document, RequestAdapterState state);
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
}
