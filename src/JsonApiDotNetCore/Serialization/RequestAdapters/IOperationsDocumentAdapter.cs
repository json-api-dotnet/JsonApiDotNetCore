using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <summary>
    /// Validates and converts a <see cref="Document" /> belonging to an atomic:operations request.
    /// </summary>
    public interface IOperationsDocumentAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="document" />.
        /// </summary>
        IList<OperationContainer> Convert(Document document, RequestAdapterState state);
    }
}
