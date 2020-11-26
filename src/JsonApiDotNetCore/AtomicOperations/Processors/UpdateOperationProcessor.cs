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
        private readonly IResourceContextProvider _resourceContextProvider;

        public UpdateOperationProcessor(IUpdateService<TResource, TId> service, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _resourceObjectBuilder = resourceObjectBuilder ?? throw new ArgumentNullException(nameof(resourceObjectBuilder));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        public async Task<AtomicResultObject> ProcessAsync(AtomicOperationObject operation, CancellationToken cancellationToken)
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
                ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(operation.SingleData.Type);
                data = _resourceObjectBuilder.Build(result, resourceContext.Attributes, resourceContext.Relationships);
            }

            return new AtomicResultObject
            {
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
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider)
            : base(service, deserializer, resourceObjectBuilder, resourceContextProvider)
        {
        }
    }
}
