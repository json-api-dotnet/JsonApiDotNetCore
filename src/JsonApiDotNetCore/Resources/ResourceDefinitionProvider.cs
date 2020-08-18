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
            _resourceContextProvider = resourceContextProvider;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IResourceDefinition Get(Type resourceType)
        {
            return (IResourceDefinition)_serviceProvider.GetService(_resourceContextProvider.GetResourceContext(resourceType).ResourceDefinitionType);
        }
    }
}
