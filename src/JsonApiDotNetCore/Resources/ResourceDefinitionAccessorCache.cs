using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Resources;

internal static class ResourceDefinitionAccessorCache
{
    private static readonly ConcurrentDictionary<ResourceType, Type> ResourceDefinitionTypeCache = new();

    private static readonly ConcurrentDictionary<Type, Func<object, IImmutableSet<IncludeElementExpression>, IImmutableSet<IncludeElementExpression>>>
        OnApplyIncludesDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, FilterExpression?, FilterExpression?>> OnApplyFilterDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, SortExpression?, SortExpression?>> OnApplySortDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, PaginationExpression?, PaginationExpression?>> OnApplyPaginationDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, SparseFieldSetExpression?, SparseFieldSetExpression?>> OnApplySparseFieldSetDelegates =
        new();

    private static readonly ConcurrentDictionary<Type, Func<object, string, object?>> GetQueryableHandlerDelegates = new();
    private static readonly ConcurrentDictionary<Type, Func<object, IIdentifiable, IDictionary<string, object?>?>> GetMetaDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task>> OnPrepareWriteAsyncDelegates =
        new();

    private static readonly
        ConcurrentDictionary<Type, Func<object, IIdentifiable, HasOneAttribute, IIdentifiable?, WriteOperationKind, CancellationToken, Task<IIdentifiable?>>>
        OnSetToOneRelationshipAsyncDelegates = new();

    private static readonly
        ConcurrentDictionary<Type, Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, WriteOperationKind, CancellationToken, Task>>
        OnSetToManyRelationshipAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task>>
        OnAddToRelationshipAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task>>
        OnRemoveFromRelationshipAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task>>
        OnWritingAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task>>
        OnWriteSucceededAsyncDelegates = new();

    private static readonly ConcurrentDictionary<Type, Action<object, IIdentifiable>> OnDeserializeDelegates = new();
    private static readonly ConcurrentDictionary<Type, Action<object, IIdentifiable>> OnSerializeDelegates = new();

    public static Type GetResourceDefinitionType(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        return ResourceDefinitionTypeCache.GetOrAdd(resourceType,
            static type => typeof(IResourceDefinition<,>).MakeGenericType(type.ClrType, type.IdentityClrType));
    }

    public static Func<object, IImmutableSet<IncludeElementExpression>, IImmutableSet<IncludeElementExpression>> GetOnApplyIncludesDelegate(
        Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnApplyIncludesDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression existingIncludes = Expression.Parameter(typeof(IImmutableSet<IncludeElementExpression>), "existingIncludes");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnApplyIncludes))!;
            MethodCallExpression call = Expression.Call(castInstance, method, existingIncludes);

            return Expression
                .Lambda<Func<object, IImmutableSet<IncludeElementExpression>, IImmutableSet<IncludeElementExpression>>>(call, instance, existingIncludes)
                .Compile();
        });
    }

    public static Func<object, FilterExpression?, FilterExpression?> GetOnApplyFilterDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnApplyFilterDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression existingFilter = Expression.Parameter(typeof(FilterExpression), "existingFilter");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnApplyFilter))!;
            MethodCallExpression call = Expression.Call(castInstance, method, existingFilter);

            return Expression.Lambda<Func<object, FilterExpression?, FilterExpression?>>(call, instance, existingFilter).Compile();
        });
    }

    public static Func<object, SortExpression?, SortExpression?> GetOnApplySortDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnApplySortDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression existingSort = Expression.Parameter(typeof(SortExpression), "existingSort");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnApplySort))!;
            MethodCallExpression call = Expression.Call(castInstance, method, existingSort);

            return Expression.Lambda<Func<object, SortExpression?, SortExpression?>>(call, instance, existingSort).Compile();
        });
    }

    public static Func<object, PaginationExpression?, PaginationExpression?> GetOnApplyPaginationDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnApplyPaginationDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression existingPagination = Expression.Parameter(typeof(PaginationExpression), "existingPagination");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnApplyPagination))!;
            MethodCallExpression call = Expression.Call(castInstance, method, existingPagination);

            return Expression.Lambda<Func<object, PaginationExpression?, PaginationExpression?>>(call, instance, existingPagination).Compile();
        });
    }

    public static Func<object, SparseFieldSetExpression?, SparseFieldSetExpression?> GetOnApplySparseFieldSetDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnApplySparseFieldSetDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression existingSparseFieldSet = Expression.Parameter(typeof(SparseFieldSetExpression), "existingSparseFieldSet");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnApplySparseFieldSet))!;
            MethodCallExpression call = Expression.Call(castInstance, method, existingSparseFieldSet);

            return Expression.Lambda<Func<object, SparseFieldSetExpression?, SparseFieldSetExpression?>>(call, instance, existingSparseFieldSet).Compile();
        });
    }

    public static Func<object, string, object?> GetGetQueryableHandlerDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return GetQueryableHandlerDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression parameterName = Expression.Parameter(typeof(string), "parameterName");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnRegisterQueryableHandlersForQueryStringParameters))!;
            MethodCallExpression callOnRegister = Expression.Call(castInstance, method);

            MethodInfo extractMethod = typeof(ResourceDefinitionAccessorCache).GetMethod(nameof(ExtractHandler), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(resourceType);

            MethodCallExpression callHelper = Expression.Call(extractMethod, callOnRegister, parameterName);

            return Expression.Lambda<Func<object, string, object?>>(callHelper, instance, parameterName).Compile();
        });
    }

    public static Func<object, IIdentifiable, IDictionary<string, object?>?> GetGetMetaDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return GetMetaDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resource = Expression.Parameter(typeof(IIdentifiable), "resource");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castResource = Expression.Convert(resource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.GetMeta))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castResource);

            return Expression.Lambda<Func<object, IIdentifiable, IDictionary<string, object?>?>>(call, instance, resource).Compile();
        });
    }

    public static Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task> GetOnPrepareWriteAsyncDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnPrepareWriteAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resource = Expression.Parameter(typeof(IIdentifiable), "resource");
            ParameterExpression writeOperation = Expression.Parameter(typeof(WriteOperationKind), "writeOperation");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castResource = Expression.Convert(resource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnPrepareWriteAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castResource, writeOperation, cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task>>(call, instance, resource, writeOperation,
                cancellationToken).Compile();
        });
    }

    public static Func<object, IIdentifiable, HasOneAttribute, IIdentifiable?, WriteOperationKind, CancellationToken, Task<IIdentifiable?>>
        GetOnSetToOneRelationshipAsyncDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnSetToOneRelationshipAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(IIdentifiable), "leftResource");
            ParameterExpression hasOneRelationship = Expression.Parameter(typeof(HasOneAttribute), "hasOneRelationship");
            ParameterExpression rightResourceId = Expression.Parameter(typeof(IIdentifiable), "rightResourceId");
            ParameterExpression writeOperation = Expression.Parameter(typeof(WriteOperationKind), "writeOperation");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castLeftResource = Expression.Convert(leftResource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnSetToOneRelationshipAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, castLeftResource, hasOneRelationship, rightResourceId, writeOperation,
                cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, HasOneAttribute, IIdentifiable?, WriteOperationKind, CancellationToken, Task<IIdentifiable?>>>(
                call, instance, leftResource, hasOneRelationship, rightResourceId, writeOperation, cancellationToken).Compile();
        });
    }

    public static Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, WriteOperationKind, CancellationToken, Task>
        GetOnSetToManyRelationshipAsyncDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnSetToManyRelationshipAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(IIdentifiable), "leftResource");
            ParameterExpression hasManyRelationship = Expression.Parameter(typeof(HasManyAttribute), "hasManyRelationship");
            ParameterExpression rightResourceIds = Expression.Parameter(typeof(ISet<IIdentifiable>), "rightResourceIds");
            ParameterExpression writeOperation = Expression.Parameter(typeof(WriteOperationKind), "writeOperation");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castLeftResource = Expression.Convert(leftResource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnSetToManyRelationshipAsync))!;

            MethodCallExpression call = Expression.Call(castInstance, method, castLeftResource, hasManyRelationship, rightResourceIds, writeOperation,
                cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, WriteOperationKind, CancellationToken, Task>>(call,
                instance, leftResource, hasManyRelationship, rightResourceIds, writeOperation, cancellationToken).Compile();
        });
    }

    public static Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task> GetOnAddToRelationshipAsyncDelegate(
        Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnAddToRelationshipAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(IIdentifiable), "leftResource");
            ParameterExpression hasManyRelationship = Expression.Parameter(typeof(HasManyAttribute), "hasManyRelationship");
            ParameterExpression rightResourceIds = Expression.Parameter(typeof(ISet<IIdentifiable>), "rightResourceIds");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castLeftResource = Expression.Convert(leftResource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnAddToRelationshipAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castLeftResource, hasManyRelationship, rightResourceIds, cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task>>(call, instance, leftResource,
                hasManyRelationship, rightResourceIds, cancellationToken).Compile();
        });
    }

    public static Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task> GetOnRemoveFromRelationshipAsyncDelegate(
        Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnRemoveFromRelationshipAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression leftResource = Expression.Parameter(typeof(IIdentifiable), "leftResource");
            ParameterExpression hasManyRelationship = Expression.Parameter(typeof(HasManyAttribute), "hasManyRelationship");
            ParameterExpression rightResourceIds = Expression.Parameter(typeof(ISet<IIdentifiable>), "rightResourceIds");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castLeftResource = Expression.Convert(leftResource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnRemoveFromRelationshipAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castLeftResource, hasManyRelationship, rightResourceIds, cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, HasManyAttribute, ISet<IIdentifiable>, CancellationToken, Task>>(call, instance, leftResource,
                hasManyRelationship, rightResourceIds, cancellationToken).Compile();
        });
    }

    public static Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task> GetOnWritingAsyncDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnWritingAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resource = Expression.Parameter(typeof(IIdentifiable), "resource");
            ParameterExpression writeOperation = Expression.Parameter(typeof(WriteOperationKind), "writeOperation");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castResource = Expression.Convert(resource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnWritingAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castResource, writeOperation, cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task>>(call, instance, resource, writeOperation,
                cancellationToken).Compile();
        });
    }

    public static Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task> GetOnWriteSucceededAsyncDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnWriteSucceededAsyncDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resource = Expression.Parameter(typeof(IIdentifiable), "resource");
            ParameterExpression writeOperation = Expression.Parameter(typeof(WriteOperationKind), "writeOperation");
            ParameterExpression cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castResource = Expression.Convert(resource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnWriteSucceededAsync))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castResource, writeOperation, cancellationToken);

            return Expression.Lambda<Func<object, IIdentifiable, WriteOperationKind, CancellationToken, Task>>(call, instance, resource, writeOperation,
                cancellationToken).Compile();
        });
    }

    public static Action<object, IIdentifiable> GetOnDeserializeDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnDeserializeDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resource = Expression.Parameter(typeof(IIdentifiable), "resource");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castResource = Expression.Convert(resource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnDeserialize))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castResource);

            return Expression.Lambda<Action<object, IIdentifiable>>(call, instance, resource).Compile();
        });
    }

    public static Action<object, IIdentifiable> GetOnSerializeDelegate(Type resourceDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(resourceDefinitionType);

        return OnSerializeDelegates.GetOrAdd(resourceDefinitionType, static type =>
        {
            Type resourceType = GetResourceType(type);

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression resource = Expression.Parameter(typeof(IIdentifiable), "resource");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castResource = Expression.Convert(resource, resourceType);
            MethodInfo method = type.GetMethod(nameof(IResourceDefinition<,>.OnSerialize))!;
            MethodCallExpression call = Expression.Call(castInstance, method, castResource);

            return Expression.Lambda<Action<object, IIdentifiable>>(call, instance, resource).Compile();
        });
    }

    private static Type GetResourceType(Type resourceDefinitionType)
    {
        return resourceDefinitionType.GetInterfaces()
            .First(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IResourceDefinition<,>))
            .GetGenericArguments()[0];
    }

    private static object? ExtractHandler<TResource>(QueryStringParameterHandlers<TResource>? handlers, string parameterName)
        where TResource : class, IIdentifiable
    {
        if (handlers != null && handlers.TryGetValue(parameterName, out Func<IQueryable<TResource>, StringValues, IQueryable<TResource>>? handler))
        {
            return handler;
        }

        return null;
    }
}
