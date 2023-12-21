using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Scans assemblies for types that implement <see cref="IIdentifiable{TId}" /> and adds them to the resource graph.
/// </summary>
internal sealed class ResourcesAssemblyScanner
{
    private readonly ResourceDescriptorAssemblyCache _assemblyCache;
    private readonly ResourceGraphBuilder _resourceGraphBuilder;

    public ResourcesAssemblyScanner(ResourceDescriptorAssemblyCache assemblyCache, ResourceGraphBuilder resourceGraphBuilder)
    {
        ArgumentGuard.NotNull(assemblyCache);
        ArgumentGuard.NotNull(resourceGraphBuilder);

        _assemblyCache = assemblyCache;
        _resourceGraphBuilder = resourceGraphBuilder;
    }

    public void DiscoverResources()
    {
        foreach (ResourceDescriptor resourceDescriptor in _assemblyCache.GetResourceDescriptors())
        {
            _resourceGraphBuilder.Add(resourceDescriptor.ResourceClrType, resourceDescriptor.IdClrType);
        }
    }
}
