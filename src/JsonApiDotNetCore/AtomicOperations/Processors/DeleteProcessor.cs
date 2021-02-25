using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    [PublicAPI]
    public class DeleteProcessor<TResource, TId> : IDeleteProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IDeleteService<TResource, TId> _service;

        public DeleteProcessor(IDeleteService<TResource, TId> service)
        {
            ArgumentGuard.NotNull(service, nameof(service));

            _service = service;
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operation, nameof(operation));

            var id = (TId)operation.Resource.GetTypedId();
            await _service.DeleteAsync(id, cancellationToken);

            return null;
        }
    }
}
