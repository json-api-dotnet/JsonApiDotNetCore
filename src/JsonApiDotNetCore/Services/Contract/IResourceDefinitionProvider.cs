using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Service to retrieve resource definitions. Goal is to encapsulate
    /// the service provider that needs to be injected for this purpose.
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