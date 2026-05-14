using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories;

internal static class ResourceRepositoryAccessorCache
{
    private static readonly ConcurrentDictionary<ResourceType, Type> ReadRepositoryTypeCache = new();
    private static readonly ConcurrentDictionary<ResourceType, Type> WriteRepositoryTypeCache = new();

    private static readonly ConcurrentDictionary<Type, Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<IIdentifiable>>>>
        GetAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, FilterExpression?, CancellationToken, Task<int>>> CountAsyncDelegates = new();

    public static Type GetReadRepositoryType(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        return ReadRepositoryTypeCache.GetOrAdd(resourceType,
            static type => typeof(IResourceReadRepository<,>).MakeGenericType(type.ClrType, type.IdentityClrType));
    }

    public static Type GetWriteRepositoryType(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        return WriteRepositoryTypeCache.GetOrAdd(resourceType,
            static type => typeof(IResourceWriteRepository<,>).MakeGenericType(type.ClrType, type.IdentityClrType));
    }

    public static Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<IIdentifiable>>> GetGetAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return GetAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            Type repositoryInterface = type.GetInterfaces().First(interfaceType =>
                interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IResourceReadRepository<,>));

            Type resourceType = repositoryInterface.GetGenericArguments()[0];

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression queryLayer = Expression.Parameter(typeof(QueryLayer), "queryLayer");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceReadRepository<,>.GetAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, queryLayer, cancellationToken);

            MethodInfo conversionMethod =
                typeof(ResourceRepositoryAccessorCache).GetMethod(nameof(ConvertGetAsyncResult), BindingFlags.Static | BindingFlags.NonPublic)!
                    .MakeGenericMethod(resourceType);

            MethodCallExpression convertedCall = Expression.Call(conversionMethod, call);

            return Expression.Lambda<Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<IIdentifiable>>>>(convertedCall, instance,
                queryLayer, cancellationToken).Compile();
        });
    }

    public static Func<object, FilterExpression?, CancellationToken, Task<int>> GetCountAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return CountAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression filter = Expression.Parameter(typeof(FilterExpression), "filter");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceReadRepository<,>.CountAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, filter, cancellationToken);

            return Expression.Lambda<Func<object, FilterExpression?, CancellationToken, Task<int>>>(call, instance, filter, cancellationToken).Compile();
        });
    }

    private static async Task<IReadOnlyCollection<IIdentifiable>> ConvertGetAsyncResult<TResource>(Task<IReadOnlyCollection<TResource>> task)
        where TResource : class, IIdentifiable
    {
        return await task;
    }
}

internal static class RepositoryAccessorCache<TResource>
    where TResource : class, IIdentifiable
{
    private static readonly ConcurrentDictionary<Type, Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<TResource>>>> GetAsyncDelegates =
        new();

    private static readonly ConcurrentDictionary<Type, Func<object, TResource, TResource, CancellationToken, Task>> CreateAsyncDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, QueryLayer, CancellationToken, Task<TResource?>>> GetForUpdateAsyncDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, TResource, TResource, CancellationToken, Task>> UpdateAsyncDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, TResource, object?, CancellationToken, Task>> SetRelationshipAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, TResource, ISet<IIdentifiable>, CancellationToken, Task>>
        RemoveFromToManyRelationshipAsyncDelegates = new();

    public static Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<TResource>>> GetGetAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return GetAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression queryLayer = Expression.Parameter(typeof(QueryLayer), "queryLayer");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceReadRepository<,>.GetAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, queryLayer, cancellationToken);

            return Expression.Lambda<Func<object, QueryLayer, CancellationToken, Task<IReadOnlyCollection<TResource>>>>(call, instance, queryLayer,
                cancellationToken).Compile();
        });
    }

    public static Func<object, TResource, TResource, CancellationToken, Task> GetCreateAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return CreateAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resourceFromRequest = Expression.Parameter(typeof(TResource), "resourceFromRequest");
            ParameterExpression resourceForDatabase = Expression.Parameter(typeof(TResource), "resourceForDatabase");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.CreateAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, resourceFromRequest, resourceForDatabase, cancellationToken);

            return Expression.Lambda<Func<object, TResource, TResource, CancellationToken, Task>>(call, instance, resourceFromRequest,
                resourceForDatabase, cancellationToken).Compile();
        });
    }

    public static Func<object, QueryLayer, CancellationToken, Task<TResource?>> GetGetForUpdateAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return GetForUpdateAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression queryLayer = Expression.Parameter(typeof(QueryLayer), "queryLayer");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.GetForUpdateAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, queryLayer, cancellationToken);

            return Expression.Lambda<Func<object, QueryLayer, CancellationToken, Task<TResource?>>>(call, instance, queryLayer, cancellationToken).Compile();
        });
    }

    public static Func<object, TResource, TResource, CancellationToken, Task> GetUpdateAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return UpdateAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resourceFromRequest = Expression.Parameter(typeof(TResource), "resourceFromRequest");
            ParameterExpression resourceFromDatabase = Expression.Parameter(typeof(TResource), "resourceFromDatabase");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.UpdateAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, resourceFromRequest, resourceFromDatabase, cancellationToken);

            return Expression.Lambda<Func<object, TResource, TResource, CancellationToken, Task>>(call, instance, resourceFromRequest,
                resourceFromDatabase, cancellationToken).Compile();
        });
    }

    public static Func<object, TResource, object?, CancellationToken, Task> GetSetRelationshipAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return SetRelationshipAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(TResource), "leftResource");
            ParameterExpression rightValue = Expression.Parameter(typeof(object), "rightValue");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.SetRelationshipAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, leftResource, rightValue, cancellationToken);

            return Expression.Lambda<Func<object, TResource, object?, CancellationToken, Task>>(call, instance, leftResource, rightValue, cancellationToken)
                .Compile();
        });
    }

    public static Func<object, TResource, ISet<IIdentifiable>, CancellationToken, Task> GetRemoveFromToManyRelationshipAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return RemoveFromToManyRelationshipAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(TResource), "leftResource");
            ParameterExpression rightResourceIds = Expression.Parameter(typeof(ISet<IIdentifiable>), "rightResourceIds");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.RemoveFromToManyRelationshipAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, leftResource, rightResourceIds, cancellationToken);

            return Expression.Lambda<Func<object, TResource, ISet<IIdentifiable>, CancellationToken, Task>>(call, instance, leftResource,
                rightResourceIds, cancellationToken).Compile();
        });
    }
}

internal static class RepositoryAccessorCache<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private static readonly ConcurrentDictionary<Type, Func<object, Type, TId, CancellationToken, Task<TResource>>> GetForCreateAsyncDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, TResource?, TId, CancellationToken, Task>> DeleteAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, TResource?, TId, ISet<IIdentifiable>, CancellationToken, Task>>
        AddToToManyRelationshipAsyncDelegates = new();

    public static Func<object, Type, TId, CancellationToken, Task<TResource>> GetGetForCreateAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return GetForCreateAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resourceClrType = Expression.Parameter(typeof(Type), "resourceClrType");
            ParameterExpression id = Expression.Parameter(typeof(TId), "id");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.GetForCreateAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, resourceClrType, id, cancellationToken);

            return Expression.Lambda<Func<object, Type, TId, CancellationToken, Task<TResource>>>(call, instance, resourceClrType, id, cancellationToken)
                .Compile();
        });
    }

    public static Func<object, TResource?, TId, CancellationToken, Task> GetDeleteAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return DeleteAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resourceFromDatabase = Expression.Parameter(typeof(TResource), "resourceFromDatabase");
            ParameterExpression id = Expression.Parameter(typeof(TId), "id");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.DeleteAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, resourceFromDatabase, id, cancellationToken);

            return Expression.Lambda<Func<object, TResource?, TId, CancellationToken, Task>>(call, instance, resourceFromDatabase, id, cancellationToken)
                .Compile();
        });
    }

    public static Func<object, TResource?, TId, ISet<IIdentifiable>, CancellationToken, Task> GetAddToToManyRelationshipAsyncDelegate(Type repositoryType)
    {
        ArgumentNullException.ThrowIfNull(repositoryType);

        return AddToToManyRelationshipAsyncDelegates.GetOrAdd(repositoryType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(TResource), "leftResource");
            ParameterExpression leftId = Expression.Parameter(typeof(TId), "leftId");
            ParameterExpression rightResourceIds = Expression.Parameter(typeof(ISet<IIdentifiable>), "rightResourceIds");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceWriteRepository<,>.AddToToManyRelationshipAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, leftResource, leftId, rightResourceIds, cancellationToken);

            return Expression.Lambda<Func<object, TResource?, TId, ISet<IIdentifiable>, CancellationToken, Task>>(call, instance, leftResource,
                leftId, rightResourceIds, cancellationToken).Compile();
        });
    }
}
