using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class CreateOperationProcessor<TResource, TId> : ICreateOperationProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ICreateService<TResource, TId> _service;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceGraph _resourceGraph;

        public CreateOperationProcessor(ICreateService<TResource, TId> service, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceGraph resourceGraph)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _resourceObjectBuilder = resourceObjectBuilder ?? throw new ArgumentNullException(nameof(resourceObjectBuilder));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
        }

        public async Task<AtomicOperation> ProcessAsync(AtomicOperation operation, CancellationToken cancellationToken)
        {
            var model = (TResource) _deserializer.CreateResourceFromObject(operation.SingleData);
            var result = await _service.CreateAsync(model, cancellationToken);

            var operationResult = new AtomicOperation
            {
                Code = AtomicOperationCode.Add
            };

            ResourceContext resourceContext = _resourceGraph.GetResourceContext(operation.SingleData.Type);

            operationResult.Data =
                _resourceObjectBuilder.Build(result, resourceContext.Attributes, resourceContext.Relationships);

            // we need to persist the original request localId so that subsequent operations
            // can locate the result of this operation by its localId
            operationResult.SingleData.LocalId = operation.SingleData.LocalId;

            return operationResult;
        }
    }

    /// <summary>
    /// Processes a single operation with code <see cref="AtomicOperationCode.Add"/> in a list of atomic operations.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class CreateOperationProcessor<TResource> : CreateOperationProcessor<TResource, int>, ICreateOperationProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public CreateOperationProcessor(ICreateService<TResource, int> service, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceGraph resourceGraph)
            : base(service, deserializer, resourceObjectBuilder, resourceGraph)
        {
        }
    }
}
