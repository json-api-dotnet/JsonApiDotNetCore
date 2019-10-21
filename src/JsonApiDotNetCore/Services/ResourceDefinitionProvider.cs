using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    internal class ResourceDefinitionProvider : IResourceDefinitionProvider
    {
        private readonly IResourceGraphExplorer _resourceContextProvider;
        private readonly IScopedServiceProvider _serviceProvider;

        public ResourceDefinitionProvider(IResourceGraphExplorer resourceContextProvider, IScopedServiceProvider serviceProvider)
        {
            _resourceContextProvider = resourceContextProvider;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IResourceDefinition Get(Type resourceType)
        {
            return (IResourceDefinition)_serviceProvider.GetService(_resourceContextProvider.GetContextEntity(resourceType).ResourceType);
        }
    }
}
