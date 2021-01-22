using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class DeleteProcessor<TResource, TId> : IDeleteProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IDeleteService<TResource, TId> _service;

        public DeleteProcessor(IDeleteService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation,
            CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var id = (TId) operation.Resource.GetTypedId();
            await _service.DeleteAsync(id, cancellationToken);

            return null;
        }
    }
}
