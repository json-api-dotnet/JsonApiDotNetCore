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
    public class RemoveFromRelationshipProcessor<TResource, TId> : IRemoveFromRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IRemoveFromRelationshipService<TResource, TId> _service;

        public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource, TId> service)
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

            await _service.RemoveFromToManyRelationshipAsync(primaryId, operation.Request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return null;
        }
    }
}
