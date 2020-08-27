using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    public interface IServiceDiscoveryFacade
    {
        /// <summary>
        /// Registers the designated assembly for discovery of JsonApiDotNetCore services and resources.
        /// </summary>
        ServiceDiscoveryFacade AddAssembly(Assembly assembly);
        /// <summary>
        /// Registers the current assembly for discovery of JsonApiDotNetCore services and resources.
        /// </summary>
        ServiceDiscoveryFacade AddCurrentAssembly();

        /// <summary>
        /// Discovers JsonApiDotNetCore services in the registered assemblies and adds them to the DI container.
        /// </summary>
        internal void DiscoverInjectables();

        /// <summary>
        /// Discovers JsonApiDotNetCore resources in the registered assemblies and adds them to the resource graph.
        /// </summary>
        internal void DiscoverResources();
    }
}
