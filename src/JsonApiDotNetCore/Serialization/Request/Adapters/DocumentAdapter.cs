using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IDocumentAdapter" />
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
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(targetedFields);
        ArgumentNullException.ThrowIfNull(documentInResourceOrRelationshipRequestAdapter);
        ArgumentNullException.ThrowIfNull(documentInOperationsRequestAdapter);

        _request = request;
        _targetedFields = targetedFields;
        _documentInResourceOrRelationshipRequestAdapter = documentInResourceOrRelationshipRequestAdapter;
        _documentInOperationsRequestAdapter = documentInOperationsRequestAdapter;
    }

    /// <inheritdoc />
    public object? Convert(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        using var adapterState = new RequestAdapterState(_request, _targetedFields);

        return adapterState.Request.Kind == EndpointKind.AtomicOperations
            ? _documentInOperationsRequestAdapter.Convert(document, adapterState)
            : _documentInResourceOrRelationshipRequestAdapter.Convert(document, adapterState);
    }
}
