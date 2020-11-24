using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class RemoveOperationProcessor<TResource, TId> : IRemoveOperationProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IDeleteService<TResource, TId> _service;

        public RemoveOperationProcessor(IDeleteService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<AtomicOperation> ProcessAsync(AtomicOperation operation, CancellationToken cancellationToken)
        {
            var stringId = operation.Ref?.Id;
            if (string.IsNullOrWhiteSpace(stringId))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "The ref.id element is required for remove operations."
                });
            }

            var id = (TId) TypeHelper.ConvertType(stringId, typeof(TId));
            await _service.DeleteAsync(id, cancellationToken);

            return null;
        }
    }

    /// <summary>
    /// Processes a single operation with code <see cref="AtomicOperationCode.Remove"/> in a list of atomic operations.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class RemoveOperationProcessor<TResource> : RemoveOperationProcessor<TResource, int>, IRemoveOperationProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public RemoveOperationProcessor(IDeleteService<TResource, int> service)
            : base(service)
        {
        }
    }
}
