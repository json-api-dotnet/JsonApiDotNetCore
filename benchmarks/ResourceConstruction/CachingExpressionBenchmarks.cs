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
public class CachingExpressionBenchmarks
{
    private static readonly SystemClock SystemClock = new();
    private readonly Dictionary<Type, Func<IIdentifiable>> _cache = new();

    public CachingExpressionBenchmarks()
    {
        var serviceContainer = new ServiceContainer();
        serviceContainer.AddService(typeof(ISystemClock), SystemClock);

        _cache[typeof(ResourceWithDefaultConstructor)] = GetInstantiateExpression<ResourceWithDefaultConstructor>(serviceContainer);
        _cache[typeof(ResourceWithParameterizedConstructor)] = GetInstantiateExpression<ResourceWithParameterizedConstructor>(serviceContainer);
    }

    [Benchmark]
    public object CachingExpression_CreateInstance()
    {
        Func<IIdentifiable> callback = _cache[typeof(ResourceWithDefaultConstructor)];

        return callback();
    }

    [Benchmark]
    public object CachingExpression_CreateParameterizedInstance()
    {
        Func<IIdentifiable> callback = _cache[typeof(ResourceWithParameterizedConstructor)];

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
