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
        /// Gets the resource metadata for the resource that is publicly exposed by the specified name. Throws an <see cref="InvalidOperationException" /> when
        /// not found.
        /// </summary>
        ResourceContext GetResourceContext(string publicName);

        /// <summary>
        /// Gets the resource metadata for the specified resource type. Throws an <see cref="InvalidOperationException" /> when not found.
        /// </summary>
        ResourceContext GetResourceContext(Type resourceType);

        /// <summary>
        /// Gets the resource metadata for the specified resource type. Throws an <see cref="InvalidOperationException" /> when not found.
        /// </summary>
        ResourceContext GetResourceContext<TResource>()
            where TResource : class, IIdentifiable;

        /// <summary>
        /// Attempts to get the resource metadata for the resource that is publicly exposed by the specified name. Returns <c>null</c> when not found.
        /// </summary>
        ResourceContext TryGetResourceContext(string publicName);

        /// <summary>
        /// Attempts to get the resource metadata for the specified resource type. Returns <c>null</c> when not found.
        /// </summary>
        ResourceContext TryGetResourceContext(Type resourceType);
    }
}
