using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources;

/// <inheritdoc />
internal sealed class CachingResourceFactory : IResourceFactory
{
    private static readonly ConcurrentDictionary<Type, CacheEntry?> FastConstructionCache = new();
    private readonly IServiceProvider _serviceProvider;

    public CachingResourceFactory(IServiceProvider serviceProvider)
    {
        ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IIdentifiable CreateInstance(Type resourceClrType)
    {
        ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

        CacheEntry? cacheEntry = FastConstructionCache.GetOrAdd(resourceClrType, CacheEntry.Create);
        return cacheEntry != null ? cacheEntry.Invocation() : SlowCreateInstance(resourceClrType);
    }

    private IIdentifiable SlowCreateInstance(Type resourceClrType)
    {
        NewExpression newExpression = SlowCreateNewExpression(resourceClrType);
        Func<IIdentifiable> invocation = ToInvocation(newExpression);

        return invocation();
    }

    /// <inheritdoc />
    public TResource CreateInstance<TResource>()
        where TResource : IIdentifiable
    {
        return (TResource)CreateInstance(typeof(TResource));
    }

    /// <inheritdoc />
    public NewExpression CreateNewExpression(Type resourceClrType)
    {
        ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

        CacheEntry? cacheEntry = FastConstructionCache.GetOrAdd(resourceClrType, CacheEntry.Create);
        return cacheEntry != null ? cacheEntry.NewExpression : SlowCreateNewExpression(resourceClrType);
    }

    private NewExpression SlowCreateNewExpression(Type resourceClrType)
    {
        var constructorArguments = new List<Expression>();

        ConstructorInfo longestConstructor = GetLongestConstructor(resourceClrType);

        foreach (ParameterInfo constructorParameter in longestConstructor.GetParameters())
        {
            try
            {
                object constructorArgument = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, constructorParameter.ParameterType);

                Expression argumentExpression = constructorArgument.CreateTupleAccessExpressionForConstant(constructorArgument.GetType());
                constructorArguments.Add(argumentExpression);
            }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of '{resourceClrType.FullName}': Parameter '{constructorParameter.Name}' could not be resolved.", exception);
            }
        }

        return Expression.New(longestConstructor, constructorArguments);
    }

    private static ConstructorInfo GetLongestConstructor(Type type)
    {
        ConstructorInfo[] constructors = type.GetConstructors().Where(constructor => !constructor.IsStatic).ToArray();

        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"No public constructor was found for '{type.FullName}'.");
        }

        ConstructorInfo bestMatch = constructors[0];
        int maxParameterLength = constructors[0].GetParameters().Length;

        for (int index = 1; index < constructors.Length; index++)
        {
            ConstructorInfo constructor = constructors[index];
            int length = constructor.GetParameters().Length;

            if (length > maxParameterLength)
            {
                bestMatch = constructor;
                maxParameterLength = length;
            }
        }

        return bestMatch;
    }

    private static Func<IIdentifiable> ToInvocation(NewExpression newExpression)
    {
        Expression<Func<IIdentifiable>> lambdaExpression = Expression.Lambda<Func<IIdentifiable>>(newExpression);
        return lambdaExpression.Compile();
    }

    private sealed class CacheEntry
    {
        public NewExpression NewExpression { get; }
        public Func<IIdentifiable> Invocation { get; }

        private CacheEntry(NewExpression newExpression, Func<IIdentifiable> invocation)
        {
            NewExpression = newExpression;
            Invocation = invocation;
        }

        public static CacheEntry? Create(Type type)
        {
            if (!HasSingleConstructorWithoutParameters(type))
            {
                return null;
            }

            NewExpression newExpression = Expression.New(type);
            Func<IIdentifiable> invocation = ToInvocation(newExpression);

            return new CacheEntry(newExpression, invocation);
        }

        private static bool HasSingleConstructorWithoutParameters(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors().Where(constructor => !constructor.IsStatic).ToArray();

            return constructors.Length == 1 && constructors[0].GetParameters().Length == 0;
        }
    }
}
