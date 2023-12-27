using System.Reflection;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Provides auto-discovery by scanning assemblies for resources and related injectables.
/// </summary>
[PublicAPI]
public sealed class ServiceDiscoveryFacade
{
    private readonly ResourceDescriptorAssemblyCache _assemblyCache;

    internal ServiceDiscoveryFacade(ResourceDescriptorAssemblyCache assemblyCache)
    {
        ArgumentGuard.NotNull(assemblyCache);

        _assemblyCache = assemblyCache;
    }

    /// <summary>
    /// Includes the calling assembly for auto-discovery of resources and related injectables.
    /// </summary>
    public ServiceDiscoveryFacade AddCurrentAssembly()
    {
        return AddAssembly(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Includes the specified assembly for auto-discovery of resources and related injectables.
    /// </summary>
    public ServiceDiscoveryFacade AddAssembly(Assembly assembly)
    {
        ArgumentGuard.NotNull(assembly);

        _assemblyCache.RegisterAssembly(assembly);
        return this;
    }
}
