using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors;

/// <inheritdoc cref="IRemoveFromRelationshipProcessor{TResource,TId}" />
[PublicAPI]
public class RemoveFromRelationshipProcessor<TResource, TId> : IRemoveFromRelationshipProcessor<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly IRemoveFromRelationshipService<TResource, TId> _service;

    public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource, TId> service)
    {
        ArgumentNullException.ThrowIfNull(service);

        _service = service;
    }

    /// <inheritdoc />
    public virtual async Task<OperationContainer?> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var leftId = (TId)operation.Resource.GetTypedId();
        ISet<IIdentifiable> rightResourceIds = operation.GetSecondaryResources();

        await _service.RemoveFromToManyRelationshipAsync(leftId!, operation.Request.Relationship!.PublicName, rightResourceIds, cancellationToken);

        return null;
    }
}
