using System.Reflection;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Scans for types like resources, services, repositories and resource definitions in an assembly and registers them to the IoC container. This is part of the resource auto-discovery process.
    /// </summary>
    public interface IServiceDiscoveryFacade
    {
        /// <summary>
        /// Scans in the specified assembly.
        /// </summary>
        ServiceDiscoveryFacade AddAssembly(Assembly assembly);
        
        /// <summary>
        /// Scans in the calling assembly.
        /// </summary>
        ServiceDiscoveryFacade AddCurrentAssembly();
    }
}
