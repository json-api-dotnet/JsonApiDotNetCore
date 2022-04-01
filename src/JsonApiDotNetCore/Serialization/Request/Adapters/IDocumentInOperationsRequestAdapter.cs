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
    IReadOnlyList<OperationContainer> Convert(Document document, RequestAdapterState state);
}
