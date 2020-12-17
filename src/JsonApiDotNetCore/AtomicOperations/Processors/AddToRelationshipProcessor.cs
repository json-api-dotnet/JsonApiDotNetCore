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
    /// Processes a single operation to add resources to a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class AddToRelationshipProcessor<TResource, TId>
        : BaseRelationshipProcessor<TResource, TId>, IAddToRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IAddToRelationshipService<TResource, TId> _service;
        private readonly IJsonApiRequest _request;

        public AddToRelationshipProcessor(IAddToRelationshipService<TResource, TId> service,
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

            await _service.AddToToManyRelationshipAsync(primaryId, _request.Relationship.PublicName, secondaryResourceIds, cancellationToken);

            return new AtomicResultObject();
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
        public AddToRelationshipProcessor(IAddToRelationshipService<TResource> service,
            IResourceFactory resourceFactory, IJsonApiRequest request, IJsonApiDeserializer deserializer)
            : base(service, resourceFactory, request, deserializer)
        {
        }
    }
}
