using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class RemoveFromRelationshipProcessor<TResource, TId> : IRemoveFromRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IRemoveFromRelationshipService<TResource, TId> _service;

        public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation,
            CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var primaryId = (TId) operation.Resource.GetTypedId();
            var secondaryResourceIds = operation.GetSecondaryResources();

            await _service.RemoveFromToManyRelationshipAsync(primaryId, operation.Request.Relationship.PublicName,
                secondaryResourceIds, cancellationToken);

            return null;
        }
    }
}
