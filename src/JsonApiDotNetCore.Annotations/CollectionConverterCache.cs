using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore;

internal static class CollectionConverterCache
{
    private static readonly ConcurrentDictionary<Type, Action<object, object>> AddDelegates = new();

    public static Action<object, object> GetAddDelegate(Type collectionType)
    {
        ArgumentNullException.ThrowIfNull(collectionType);

        return AddDelegates.GetOrAdd(collectionType, static type =>
        {
            MethodInfo? addMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "Add" && method.GetParameters().Length == 1);

            if (addMethod == null)
            {
                // We search for a public instance method named 'Add' that takes exactly one parameter.
                // This matches the C# language rules for collection initializers, which look for
                // an accessible 'Add' method on a type that implements IEnumerable.
                throw new InvalidOperationException($"Type '{type.Name}' does not have a public 'Add' method with a single parameter.");
            }

            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression item = Expression.Parameter(typeof(object), "item");

            UnaryExpression castInstance = Expression.Convert(instance, type);
            UnaryExpression castItem = Expression.Convert(item, addMethod.GetParameters()[0].ParameterType);

            Expression call = Expression.Call(castInstance, addMethod, castItem);

            if (call.Type != typeof(void))
            {
                // Some collections (like HashSet<T>) have an Add method that returns a value (e.g. bool).
                // Since our delegate type is Action<object, object>, we must discard the return value
                // by wrapping the call in a block that returns void.
                call = Expression.Block(typeof(void), call);
            }

            return Expression.Lambda<Action<object, object>>(call, instance, item).Compile();
        });
    }
}
