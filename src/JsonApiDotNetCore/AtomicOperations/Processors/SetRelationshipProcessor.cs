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
    /// Processes a single operation to perform a complete replacement of a relationship on an existing resource.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class SetRelationshipProcessor<TResource, TId> 
        : BaseRelationshipProcessor<TResource, TId>, ISetRelationshipProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ISetRelationshipService<TResource, TId> _service;
        private readonly IJsonApiRequest _request;

        public SetRelationshipProcessor(ISetRelationshipService<TResource, TId> service,
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
            object relationshipValueToAssign = null;

            if (operation.SingleData != null)
            {
                relationshipValueToAssign = _deserializer.CreateResourceFromObject(operation.SingleData);
            }

            if (operation.ManyData != null)
            {
                relationshipValueToAssign = GetSecondaryResourceIds(operation);
            }

            await _service.SetRelationshipAsync(primaryId, _request.Relationship.PublicName, relationshipValueToAssign, cancellationToken);

            return new AtomicResultObject();
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
        public SetRelationshipProcessor(ISetRelationshipService<TResource, int> service,
            IResourceFactory resourceFactory, IJsonApiRequest request, IJsonApiDeserializer deserializer)
            : base(service, resourceFactory, request, deserializer)
        {
        }
    }
}
