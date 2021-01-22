using System;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
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
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation,
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

            return newResource == null ? null : operation.WithResource(newResource);
        }
    }
}
