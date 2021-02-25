using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Hooks.Internal.Discovery
{
    /// <summary>
    /// A singleton service for a particular TResource that stores a field of enums that represents which resource hooks have been implemented for that
    /// particular resource.
    /// </summary>
    public interface IHooksDiscovery<TResource> : IHooksDiscovery
        where TResource : class, IIdentifiable
    {
    }

    public interface IHooksDiscovery
    {
        /// <summary>
        /// A list of the implemented hooks for resource TResource
        /// </summary>
        /// <value>
        /// The implemented hooks.
        /// </value>
        ResourceHook[] ImplementedHooks { get; }

        ResourceHook[] DatabaseValuesEnabledHooks { get; }
        ResourceHook[] DatabaseValuesDisabledHooks { get; }
    }
}
