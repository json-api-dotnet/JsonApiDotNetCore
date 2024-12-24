using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors;

/// <inheritdoc cref="ICreateProcessor{TResource,TId}" />
[PublicAPI]
public class CreateProcessor<TResource, TId> : ICreateProcessor<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly ICreateService<TResource, TId> _service;
    private readonly ILocalIdTracker _localIdTracker;

    public CreateProcessor(ICreateService<TResource, TId> service, ILocalIdTracker localIdTracker)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(localIdTracker);

        _service = service;
        _localIdTracker = localIdTracker;
    }

    /// <inheritdoc />
    public virtual async Task<OperationContainer?> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        TResource? newResource = await _service.CreateAsync((TResource)operation.Resource, cancellationToken);

        if (operation.Resource.LocalId != null)
        {
            string serverId = newResource != null ? newResource.StringId! : operation.Resource.StringId!;
            _localIdTracker.Assign(operation.Resource.LocalId, operation.Request.PrimaryResourceType!, serverId);
        }

        return newResource == null ? null : operation.WithResource(newResource);
    }
}
