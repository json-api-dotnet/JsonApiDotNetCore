using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Abstracts away the creation of the corresponding generic type and usage
    /// of the service provider in order to get a <see cref="ResourceDefinition{TResource}"/>
    /// service.
    /// </summary>
    internal class ResourceDefinitionProvider : IResourceDefinitionProvider
    {
        private readonly IScopedServiceProvider _sp;
        private readonly IContextEntityProvider _rcp;

        public ResourceDefinitionProvider(IContextEntityProvider resourceContextProvider, IScopedServiceProvider serviceProvider)
        {
            _sp = serviceProvider;
            _rcp = resourceContextProvider;
        }

        /// <inheritdoc/>
        public IResourceDefinition Get(Type resourceType)
        {
            return (IResourceDefinition)_sp.GetService(_rcp.GetContextEntity(resourceType).ResourceType);
        }
    }
}
