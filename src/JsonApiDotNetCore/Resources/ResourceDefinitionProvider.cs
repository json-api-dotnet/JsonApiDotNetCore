using System;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc/>
    internal sealed class ResourceDefinitionProvider : IResourceDefinitionProvider
    {
        private readonly IResourceGraph _resourceContextProvider;
        private readonly IScopedServiceProvider _serviceProvider;

        public ResourceDefinitionProvider(IResourceGraph resourceContextProvider, IScopedServiceProvider serviceProvider)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public IResourceDefinition Get(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            return (IResourceDefinition)_serviceProvider.GetService(resourceContext.ResourceDefinitionType);
        }
    }
}
