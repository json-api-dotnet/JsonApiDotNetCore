using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class SetRelationshipProcessor<TResource, TId> : ISetRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ISetRelationshipService<TResource, TId> _service;

        public SetRelationshipProcessor(ISetRelationshipService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation,
            CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var primaryId = (TId) operation.Resource.GetTypedId();
            object secondaryResourceIds = operation.GetSecondaryResourceIdOrIds();

            await _service.SetRelationshipAsync(primaryId, operation.Request.Relationship.PublicName,
                secondaryResourceIds, cancellationToken);

            return null;
        }
    }
}
