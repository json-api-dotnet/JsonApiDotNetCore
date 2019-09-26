using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Contracts
{
    public interface IContextEntityProvider
    {
        /// <summary>
        /// Get the resource metadata by the DbSet property name
        /// </summary>
        ContextEntity GetContextEntity(string exposedResourceName);

        /// <summary>
        /// Get the resource metadata by the resource type
        /// </summary>
        ContextEntity GetContextEntity(Type resourceType);

        /// <summary>
        /// Get the resource metadata by the resource type
        /// </summary>
        ContextEntity GetContextEntity<TResource>() where TResource : class, IIdentifiable;
    }
}