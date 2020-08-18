using System.Reflection;

namespace JsonApiDotNetCore.Configuration
{
    public interface IServiceDiscoveryFacade
    {
        ServiceDiscoveryFacade AddAssembly(Assembly assembly);
        ServiceDiscoveryFacade AddCurrentAssembly();
    }
}