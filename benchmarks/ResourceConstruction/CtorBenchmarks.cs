using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;

namespace Benchmarks.ResourceConstruction;

// ReSharper disable once ClassCanBeSealed.Global
[MarkdownExporter]
[SimpleJob(3, 10, 20)]
[MemoryDiagnoser]
public class CtorBenchmarks
{
    private static readonly IServiceProvider ServiceProvider;

    static CtorBenchmarks()
    {
        var serviceContainer = new ServiceContainer();
        serviceContainer.AddService(typeof(ISystemClock), new SystemClock());

        ServiceProvider = serviceContainer;
    }

    [Benchmark]
    public object ResourceFactory_CreateInstance()
    {
        IResourceFactory factory = new ResourceFactory(ServiceProvider);

        return factory.CreateInstance<ResourceWithDefaultConstructor>();
    }

    [Benchmark]
    public object ResourceFactory_CreateParameterizedInstance()
    {
        IResourceFactory factory = new ResourceFactory(ServiceProvider);

        return factory.CreateInstance<ResourceWithParameterizedConstructor>();
    }

    [Benchmark]
    public object ExpressionResourceFactory_CreateInstance()
    {
        IResourceFactory factory = new ExpressionResourceFactory(ServiceProvider);

        return factory.CreateInstance<ResourceWithDefaultConstructor>();
    }

    [Benchmark]
    public object ExpressionResourceFactory_CreateParameterizedInstance()
    {
        IResourceFactory factory = new ExpressionResourceFactory(ServiceProvider);

        return factory.CreateInstance<ResourceWithParameterizedConstructor>();
    }

    [Benchmark]
    public object CachingResourceFactory_CreateInstance()
    {
        IResourceFactory factory = new CachingResourceFactory(ServiceProvider);

        return factory.CreateInstance<ResourceWithDefaultConstructor>();
    }

    [Benchmark]
    public object CachingResourceFactory_CreateParameterizedInstance()
    {
        IResourceFactory factory = new CachingResourceFactory(ServiceProvider);

        return factory.CreateInstance<ResourceWithParameterizedConstructor>();
    }
}
