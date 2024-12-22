using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors;

/// <inheritdoc cref="IAddToRelationshipProcessor{TResource,TId}" />
[PublicAPI]
public class AddToRelationshipProcessor<TResource, TId> : IAddToRelationshipProcessor<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly IAddToRelationshipService<TResource, TId> _service;

    public AddToRelationshipProcessor(IAddToRelationshipService<TResource, TId> service)
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

        await _service.AddToToManyRelationshipAsync(leftId!, operation.Request.Relationship!.PublicName, rightResourceIds, cancellationToken);

        return null;
    }
}
