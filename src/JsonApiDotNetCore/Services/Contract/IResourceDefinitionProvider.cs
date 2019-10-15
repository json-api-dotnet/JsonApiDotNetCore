using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
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
        /// <param name="resourceType"></param>
        IResourceDefinition Get(Type resourceType);
    }
}