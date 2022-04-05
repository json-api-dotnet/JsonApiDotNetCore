using System.ComponentModel.Design;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;

namespace Benchmarks.ResourceConstruction;

// ReSharper disable once ClassCanBeSealed.Global
[MarkdownExporter]
[SimpleJob(3, 10, 20)]
[MemoryDiagnoser]
public class ExpressionBenchmarks
{
    private static readonly SystemClock SystemClock = new();
    private readonly IServiceProvider _serviceProvider;

    public ExpressionBenchmarks()
    {
        var serviceContainer = new ServiceContainer();
        serviceContainer.AddService(typeof(ISystemClock), SystemClock);

        _serviceProvider = serviceContainer;
    }

    [Benchmark]
    public object Expression_CreateInstance()
    {
        Func<IIdentifiable> callback = GetInstantiateExpression<ResourceWithDefaultConstructor>(_serviceProvider);

        return callback();
    }

    [Benchmark]
    public object Expression_CreateParameterizedInstance()
    {
        Func<IIdentifiable> callback = GetInstantiateExpression<ResourceWithParameterizedConstructor>(_serviceProvider);

        return callback();
    }

    private static Func<IIdentifiable> GetInstantiateExpression<TResource>(IServiceProvider serviceProvider)
    {
        IResourceFactory factory = ResourceFactoryFactory.Create(serviceProvider);
        NewExpression newExpression = factory.CreateNewExpression(typeof(TResource));
        Expression<Func<IIdentifiable>> lambdaExpression = Expression.Lambda<Func<IIdentifiable>>(newExpression);
        return lambdaExpression.Compile();
    }
}
