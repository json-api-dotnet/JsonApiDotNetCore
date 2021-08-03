using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Responsible for getting <see cref="ResourceContext" />s from the <see cref="ResourceGraph" />.
    /// </summary>
    public interface IResourceContextProvider
    {
        /// <summary>
        /// Gets the metadata for all registered resources.
        /// </summary>
        IReadOnlySet<ResourceContext> GetResourceContexts();

        /// <summary>
        /// Gets the resource metadata for the resource that is publicly exposed by the specified name.
        /// </summary>
        ResourceContext GetResourceContext(string publicName);

        /// <summary>
        /// Gets the resource metadata for the specified resource type.
        /// </summary>
        ResourceContext GetResourceContext(Type resourceType);

        /// <summary>
        /// Gets the resource metadata for the specified resource type.
        /// </summary>
        ResourceContext GetResourceContext<TResource>()
            where TResource : class, IIdentifiable;
    }
}
