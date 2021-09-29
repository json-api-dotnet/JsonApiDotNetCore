using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc />
    public sealed class DocumentAdapter : IDocumentAdapter
    {
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceDocumentAdapter _resourceDocumentAdapter;
        private readonly IOperationsDocumentAdapter _operationsDocumentAdapter;

        public DocumentAdapter(IJsonApiRequest request, ITargetedFields targetedFields, IResourceDocumentAdapter resourceDocumentAdapter,
            IOperationsDocumentAdapter operationsDocumentAdapter)
        {
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
            ArgumentGuard.NotNull(resourceDocumentAdapter, nameof(resourceDocumentAdapter));
            ArgumentGuard.NotNull(operationsDocumentAdapter, nameof(operationsDocumentAdapter));

            _request = request;
            _targetedFields = targetedFields;
            _resourceDocumentAdapter = resourceDocumentAdapter;
            _operationsDocumentAdapter = operationsDocumentAdapter;
        }

        /// <inheritdoc />
        public object Convert(Document document)
        {
            ArgumentGuard.NotNull(document, nameof(document));

            using var context = new RequestAdapterState(_request, _targetedFields);

            return context.Request.Kind == EndpointKind.AtomicOperations
                ? _operationsDocumentAdapter.Convert(document, context)
                : _resourceDocumentAdapter.Convert(document, context);
        }
    }
}
