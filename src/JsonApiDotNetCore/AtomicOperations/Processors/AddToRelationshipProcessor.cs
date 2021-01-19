using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to add resources to a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class AddToRelationshipProcessor<TResource, TId>
        : BaseRelationshipProcessor, IAddToRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IAddToRelationshipService<TResource, TId> _service;

        public AddToRelationshipProcessor(IAddToRelationshipService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var primaryId = (TId) operation.Resource.GetTypedId();
            var secondaryResourceIds = GetSecondaryResourceIds(operation);

            await _service.AddToToManyRelationshipAsync(primaryId, operation.Request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return null;
        }
    }

    /// <summary>
    /// Processes a single operation to add resources to a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class AddToRelationshipProcessor<TResource>
        : AddToRelationshipProcessor<TResource, int>, IAddToRelationshipProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public AddToRelationshipProcessor(IAddToRelationshipService<TResource> service)
            : base(service)
        {
        }
    }
}
