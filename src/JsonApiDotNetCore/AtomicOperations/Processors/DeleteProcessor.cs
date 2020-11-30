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
    public class DeleteProcessor<TResource, TId> : IDeleteProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IDeleteService<TResource, TId> _service;

        public DeleteProcessor(IDeleteService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<AtomicResultObject> ProcessAsync(AtomicOperationObject operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var stringId = operation.Ref.Id;
            if (stringId == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "The ref.id element is required for remove operations."
                });
            }

            var id = (TId) TypeHelper.ConvertType(stringId, typeof(TId));
            await _service.DeleteAsync(id, cancellationToken);

            return new AtomicResultObject();
        }
    }

    /// <summary>
    /// Processes a single operation to delete an existing resource.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class DeleteProcessor<TResource> : DeleteProcessor<TResource, int>, IDeleteProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public DeleteProcessor(IDeleteService<TResource, int> service)
            : base(service)
        {
        }
    }
}
