using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to remove resources from a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class RemoveFromRelationshipProcessor<TResource, TId>
        : BaseRelationshipProcessor, IRemoveFromRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IRemoveFromRelationshipService<TResource, TId> _service;

        public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var primaryId = (TId) operation.Resource.GetTypedId();
            var secondaryResourceIds = GetSecondaryResourceIds(operation);

            await _service.RemoveFromToManyRelationshipAsync(primaryId,  operation.Request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return null;
        }
    }

    /// <summary>
    /// Processes a single operation to add resources to a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class RemoveFromRelationshipProcessor<TResource>
        : RemoveFromRelationshipProcessor<TResource, int>, IAddToRelationshipProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource> service)
            : base(service)
        {
        }
    }
}
