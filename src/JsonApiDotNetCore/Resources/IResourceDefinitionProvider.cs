using System;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Retrieves a <see cref="ResourceDefinition{TResource}"/> from the DI container.
    /// Abstracts away the creation of the corresponding generic type and usage
    /// of the service provider to do so.
    /// </summary>
    public interface IResourceDefinitionProvider
    {
        /// <summary>
        /// Retrieves the resource definition associated to <paramref name="resourceType"/>.
        /// </summary>
        IResourceDefinition Get(Type resourceType);
    }
}
