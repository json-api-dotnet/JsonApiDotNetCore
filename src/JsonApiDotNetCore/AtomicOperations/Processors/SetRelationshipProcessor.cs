using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to perform a complete replacement of a relationship on an existing resource.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class SetRelationshipProcessor<TResource, TId> 
        : BaseRelationshipProcessor, ISetRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ISetRelationshipService<TResource, TId> _service;

        public SetRelationshipProcessor(ISetRelationshipService<TResource, TId> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var primaryId = (TId) operation.Resource.GetTypedId();
            object secondaryResourceIds = GetSecondaryResourceIdOrIds(operation);

            await _service.SetRelationshipAsync(primaryId, operation.Request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return null;
        }
    }

    /// <summary>
    /// Processes a single operation to perform a complete replacement of a relationship on an existing resource.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class SetRelationshipProcessor<TResource>
        : SetRelationshipProcessor<TResource, int>, IUpdateProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public SetRelationshipProcessor(ISetRelationshipService<TResource> service)
            : base(service)
        {
        }
    }
}
