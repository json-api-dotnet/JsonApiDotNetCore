using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public class UpdateOperationProcessor<TResource, TId> : IUpdateOperationProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<TResource, TId> _service;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceGraph _resourceGraph;

        public UpdateOperationProcessor(IUpdateService<TResource, TId> service, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceGraph resourceGraph)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _resourceObjectBuilder = resourceObjectBuilder ?? throw new ArgumentNullException(nameof(resourceObjectBuilder));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
        }

        public async Task<AtomicOperation> ProcessAsync(AtomicOperation operation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(operation?.SingleData?.Id))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "The data.id element is required for replace operations."
                });
            }

            var model = (TResource) _deserializer.CreateResourceFromObject(operation.SingleData);

            var result = await _service.UpdateAsync(model.Id, model, cancellationToken);

            ResourceObject data = null;

            if (result != null)
            {
                ResourceContext resourceContext = _resourceGraph.GetResourceContext(operation.SingleData.Type);
                data = _resourceObjectBuilder.Build(result, resourceContext.Attributes, resourceContext.Relationships);
            }

            return new AtomicOperation
            {
                Code = AtomicOperationCode.Update,
                Data = data
            };
        }
    }

    /// <summary>
    /// Processes a single operation with code <see cref="AtomicOperationCode.Update"/> in a list of atomic operations.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class UpdateOperationProcessor<TResource> : UpdateOperationProcessor<TResource, int>, IUpdateOperationProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public UpdateOperationProcessor(IUpdateService<TResource, int> service, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceGraph resourceGraph)
            : base(service, deserializer, resourceObjectBuilder, resourceGraph)
        {
        }
    }
}
