using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <summary>
    /// Validates and converts a <see cref="Document" /> belonging to a resource or relationship request.
    /// </summary>
    public interface IDocumentInResourceOrRelationshipRequestAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="document" />.
        /// </summary>
        object Convert(Document document, RequestAdapterState state);
    }
}
