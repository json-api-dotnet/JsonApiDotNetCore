using System.ComponentModel.Design;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;

namespace Benchmarks.ResourceConstruction;

// ReSharper disable once ClassCanBeSealed.Global
[MarkdownExporter]
[SimpleJob(3, 10, 20)]
[MemoryDiagnoser]
public class ActivatorBenchmarks
{
    private static readonly SystemClock SystemClock = new();
    private readonly IServiceProvider _serviceProvider;

    public ActivatorBenchmarks()
    {
        var serviceContainer = new ServiceContainer();
        serviceContainer.AddService(typeof(ISystemClock), SystemClock);

        _serviceProvider = serviceContainer;
    }

    /*
    [Benchmark]
    public object Activator_CreateInstance()
    {
        IResourceFactory factory = ResourceFactoryFactory.Create(_serviceProvider);

        return factory.CreateInstance<ResourceWithDefaultConstructor>();
    }
    */

    [Benchmark]
    public object Activator_CreateParameterizedInstance()
    {
        IResourceFactory factory = ResourceFactoryFactory.Create(_serviceProvider);

        return factory.CreateInstance<ResourceWithParameterizedConstructor>();
    }
    
    /*
    [Benchmark]
    public object Activator_Both()
    {
        IResourceFactory factory = ResourceFactoryFactory.Create(_serviceProvider);

        var obj1 = factory.CreateInstance<ResourceWithDefaultConstructor>();
        var obj2 = factory.CreateInstance<ResourceWithParameterizedConstructor>();

        return (obj1, obj2);
    }
    */
}
