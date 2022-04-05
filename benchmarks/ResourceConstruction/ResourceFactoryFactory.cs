using JsonApiDotNetCore.Resources;

#pragma warning disable AV1008 // Class should not be static

namespace Benchmarks.ResourceConstruction;

internal static class ResourceFactoryFactory
{
    public static IResourceFactory Create(IServiceProvider serviceProvider)
    {
        //return new ResourceFactory(serviceProvider);
        //return new ExpressionResourceFactory(serviceProvider);
        return new CachingResourceFactory(serviceProvider);
    }
}
