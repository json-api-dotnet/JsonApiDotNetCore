using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc />
public sealed class DocumentAdapter : IDocumentAdapter
{
    private readonly IJsonApiRequest _request;
    private readonly ITargetedFields _targetedFields;
    private readonly IDocumentInResourceOrRelationshipRequestAdapter _documentInResourceOrRelationshipRequestAdapter;
    private readonly IDocumentInOperationsRequestAdapter _documentInOperationsRequestAdapter;

    public DocumentAdapter(IJsonApiRequest request, ITargetedFields targetedFields,
        IDocumentInResourceOrRelationshipRequestAdapter documentInResourceOrRelationshipRequestAdapter,
        IDocumentInOperationsRequestAdapter documentInOperationsRequestAdapter)
    {
        ArgumentGuard.NotNull(request);
        ArgumentGuard.NotNull(targetedFields);
        ArgumentGuard.NotNull(documentInResourceOrRelationshipRequestAdapter);
        ArgumentGuard.NotNull(documentInOperationsRequestAdapter);

        _request = request;
        _targetedFields = targetedFields;
        _documentInResourceOrRelationshipRequestAdapter = documentInResourceOrRelationshipRequestAdapter;
        _documentInOperationsRequestAdapter = documentInOperationsRequestAdapter;
    }

    /// <inheritdoc />
    public object? Convert(Document document)
    {
        ArgumentGuard.NotNull(document);

        using var adapterState = new RequestAdapterState(_request, _targetedFields);

        return adapterState.Request.Kind == EndpointKind.AtomicOperations
            ? _documentInOperationsRequestAdapter.Convert(document, adapterState)
            : _documentInResourceOrRelationshipRequestAdapter.Convert(document, adapterState);
    }
}
