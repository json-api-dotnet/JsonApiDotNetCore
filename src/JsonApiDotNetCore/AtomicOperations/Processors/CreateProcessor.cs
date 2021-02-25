using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    [PublicAPI]
    public class CreateProcessor<TResource, TId> : ICreateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ICreateService<TResource, TId> _service;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IResourceContextProvider _resourceContextProvider;

        public CreateProcessor(ICreateService<TResource, TId> service, ILocalIdTracker localIdTracker, IResourceContextProvider resourceContextProvider)
        {
            ArgumentGuard.NotNull(service, nameof(service));
            ArgumentGuard.NotNull(localIdTracker, nameof(localIdTracker));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));

            _service = service;
            _localIdTracker = localIdTracker;
            _resourceContextProvider = resourceContextProvider;
        }

        /// <inheritdoc />
        public virtual async Task<OperationContainer> ProcessAsync(OperationContainer operation, CancellationToken cancellationToken)
        {
            ArgumentGuard.NotNull(operation, nameof(operation));

            TResource newResource = await _service.CreateAsync((TResource)operation.Resource, cancellationToken);

            if (operation.Resource.LocalId != null)
            {
                string serverId = newResource != null ? newResource.StringId : operation.Resource.StringId;
                ResourceContext resourceContext = _resourceContextProvider.GetResourceContext<TResource>();

                _localIdTracker.Assign(operation.Resource.LocalId, resourceContext.PublicName, serverId);
            }

            return newResource == null ? null : operation.WithResource(newResource);
        }
    }
}
