using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors;

/// <inheritdoc cref="IDeleteProcessor{TResource,TId}" />
[PublicAPI]
public class DeleteProcessor<TResource, TId> : IDeleteProcessor<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly IDeleteService<TResource, TId> _service;

    public DeleteProcessor(IDeleteService<TResource, TId> service)
    {
        ArgumentNullException.ThrowIfNull(service);

        _service = service;
    }

    /// <inheritdoc />
    public virtual async Task<OperationContainer?> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var id = (TId)operation.Resource.GetTypedId();
#pragma warning disable CS8607 // A possible null value may not be used for a type marked with [NotNull] or [DisallowNull]
        // Justification: Temporary workaround for R# bug at https://youtrack.jetbrains.com/issue/RSRP-503026.
        await _service.DeleteAsync(id, cancellationToken);
#pragma warning restore CS8607 // A possible null value may not be used for a type marked with [NotNull] or [DisallowNull]

        return null;
    }
}
