using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    public interface IServiceDiscoveryFacade
    {
        ServiceDiscoveryFacade AddAssembly(Assembly assembly);
        ServiceDiscoveryFacade AddCurrentAssembly();
    }
}