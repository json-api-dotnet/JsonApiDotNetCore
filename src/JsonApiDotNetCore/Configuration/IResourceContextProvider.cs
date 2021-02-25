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
        /// Gets all registered resource contexts.
        /// </summary>
        IReadOnlyCollection<ResourceContext> GetResourceContexts();

        /// <summary>
        /// Gets the resource metadata for the specified exposed resource name.
        /// </summary>
        ResourceContext GetResourceContext(string resourceName);

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
