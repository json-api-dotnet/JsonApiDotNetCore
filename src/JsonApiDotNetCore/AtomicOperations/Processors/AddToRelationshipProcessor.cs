using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    [PublicAPI]
    public class AddToRelationshipProcessor<TResource, TId> : IAddToRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IAddToRelationshipService<TResource, TId> _service;

        public AddToRelationshipProcessor(IAddToRelationshipService<TResource, TId> service)
        {
            ArgumentGuard.NotNull(service, nameof(service));

            _service = service;
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operation, nameof(operation));

            var primaryId = (TId)operation.Resource.GetTypedId();
            ISet<IIdentifiable> secondaryResourceIds = operation.GetSecondaryResources();

            await _service.AddToToManyRelationshipAsync(primaryId, operation.Request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return null;
        }
    }
}
