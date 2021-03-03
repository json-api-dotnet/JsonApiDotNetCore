using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    [PublicAPI]
    public class UpdateProcessor<TResource, TId> : IUpdateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<TResource, TId> _service;

        public UpdateProcessor(IUpdateService<TResource, TId> service)
        {
            ArgumentGuard.NotNull(service, nameof(service));

            _service = service;
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operation, nameof(operation));

            var resource = (TResource)operation.Resource;
            TResource updated = await _service.UpdateAsync(resource.Id, resource, cancellationToken);

            return updated == null ? null : operation.WithResource(updated);
        }
    }
}
