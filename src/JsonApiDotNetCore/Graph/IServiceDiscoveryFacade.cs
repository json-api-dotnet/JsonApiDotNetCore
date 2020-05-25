using JsonApiDotNetCore.Models.Fluent;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    public interface IServiceDiscoveryFacade
    {
        ServiceDiscoveryFacade AddResourceMapping<TResource>(ResourceMapping<TResource> resourceMapping) where TResource: class;
        ServiceDiscoveryFacade AddAssembly(Assembly assembly);
        ServiceDiscoveryFacade AddCurrentAssembly();
    }
}
