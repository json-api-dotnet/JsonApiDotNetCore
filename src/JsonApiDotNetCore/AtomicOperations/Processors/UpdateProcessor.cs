using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class UpdateProcessor<TResource, TId> : IUpdateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<TResource, TId> _service;

        public UpdateProcessor(IUpdateService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation,
            CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var resource = (TResource) operation.Resource;
            var updated = await _service.UpdateAsync(resource.Id, resource, cancellationToken);

            return updated == null ? null : operation.WithResource(updated);
        }
    }
}
