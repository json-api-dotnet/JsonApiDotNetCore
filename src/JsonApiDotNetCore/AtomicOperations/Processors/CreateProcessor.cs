using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to create a new resource with attributes, relationships or both.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class CreateProcessor<TResource, TId> : ICreateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ICreateService<TResource, TId> _service;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IResourceContextProvider _resourceContextProvider;

        public CreateProcessor(ICreateService<TResource, TId> service, ILocalIdTracker localIdTracker,
            IResourceContextProvider resourceContextProvider)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        /// <inheritdoc />
        public async Task<IIdentifiable> ProcessAsync(OperationContainer operation,
            CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var newResource = await _service.CreateAsync((TResource) operation.Resource, cancellationToken);

            if (operation.Resource.LocalId != null)
            {
                var serverId = newResource != null ? newResource.StringId : operation.Resource.StringId;
                var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();

                _localIdTracker.Assign(operation.Resource.LocalId, resourceContext.PublicName, serverId);
            }

            return newResource;
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
        public CreateProcessor(ICreateService<TResource> service, ILocalIdTracker localIdTracker,
            IResourceContextProvider resourceContextProvider)
            : base(service, localIdTracker, resourceContextProvider)
        {
        }
    }
}
