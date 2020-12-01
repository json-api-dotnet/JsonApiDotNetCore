using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to create a new resource with attributes, relationships or both.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class CreateProcessor<TResource, TId> : BaseAtomicOperationProcessor, ICreateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ICreateService<TResource, TId> _service;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceContextProvider _resourceContextProvider;

        public CreateProcessor(ICreateService<TResource, TId> service, IJsonApiOptions options,
            IObjectModelValidator validator, ILocalIdTracker localIdTracker, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider)
            : base(options, validator)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _resourceObjectBuilder = resourceObjectBuilder ?? throw new ArgumentNullException(nameof(resourceObjectBuilder));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        /// <inheritdoc />
        public async Task<AtomicResultObject> ProcessAsync(AtomicOperationObject operation,
            CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var model = (TResource) _deserializer.CreateResourceFromObject(operation.SingleData);
            ValidateModelState(model);

            var newResource = await _service.CreateAsync(model, cancellationToken);

            if (operation.SingleData.Lid != null)
            {
                var serverId = newResource == null ? operation.SingleData.Id : newResource.StringId;
                _localIdTracker.Assign(operation.SingleData.Lid, operation.SingleData.Type, serverId);
            }

            if (newResource != null)
            {
                ResourceContext resourceContext =
                    _resourceContextProvider.GetResourceContext(operation.SingleData.Type);

                return new AtomicResultObject
                {
                    Data = _resourceObjectBuilder.Build(newResource, resourceContext.Attributes,
                        resourceContext.Relationships)
                };
            }

            return new AtomicResultObject();
        }
    }

    /// <summary>
    /// Processes a single operation to create a new resource with attributes, relationships or both.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class CreateProcessor<TResource>
        : CreateProcessor<TResource, int>, ICreateProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public CreateProcessor(ICreateService<TResource> service, IJsonApiOptions options,
            IObjectModelValidator validator, ILocalIdTracker localIdTracker, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider)
            : base(service, options, validator, localIdTracker, deserializer, resourceObjectBuilder,
                resourceContextProvider)
        {
        }
    }
}
