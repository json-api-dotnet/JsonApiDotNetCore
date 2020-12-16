using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to remove resources from a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class RemoveFromRelationshipProcessor<TResource, TId>
        : BaseRelationshipProcessor<TResource, TId>, IRemoveFromRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IRemoveFromRelationshipService<TResource, TId> _service;
        private readonly IJsonApiRequest _request;

        public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource, TId> service,
            IResourceFactory resourceFactory, IJsonApiRequest request, IJsonApiDeserializer deserializer)
            : base(resourceFactory, deserializer, request)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <inheritdoc />
        public async Task<AtomicResultObject> ProcessAsync(AtomicOperationObject operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var primaryId = GetPrimaryId(operation.Ref.Id);
            var secondaryResourceIds = GetSecondaryResourceIds(operation);

            await _service.RemoveFromToManyRelationshipAsync(primaryId, _request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return new AtomicResultObject();
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
        public RemoveFromRelationshipProcessor(IRemoveFromRelationshipService<TResource> service,
            IResourceFactory resourceFactory, IJsonApiRequest request, IJsonApiDeserializer deserializer)
            : base(service, resourceFactory, request, deserializer)
        {
        }
    }
}
