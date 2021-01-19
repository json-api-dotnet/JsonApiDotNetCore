using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to update the attributes and/or relationships of an existing resource.
    /// Only the values of sent attributes are replaced. And only the values of sent relationships are replaced.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class UpdateProcessor<TResource, TId> : IUpdateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<TResource, TId> _service;

        public UpdateProcessor(IUpdateService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var resource = (TResource) operation.Resource;
            var updated = await _service.UpdateAsync(resource.Id, resource, cancellationToken);

            return updated == null ? null : operation.WithResource(updated);
        }
    }

    /// <summary>
    /// Processes a single operation to update the attributes and/or relationships of an existing resource.
    /// Only the values of sent attributes are replaced. And only the values of sent relationships are replaced.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class UpdateProcessor<TResource>
        : UpdateProcessor<TResource, int>, IUpdateProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public UpdateProcessor(IUpdateService<TResource> service)
            : base(service)
        {
        }
    }
}
