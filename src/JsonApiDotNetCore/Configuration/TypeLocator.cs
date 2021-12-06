using System.Reflection;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Used to locate types and facilitate resource auto-discovery.
/// </summary>
internal sealed class TypeLocator
{
    // As a reminder, the following terminology is used for generic types:
    // non-generic          string
    // generic
    //     unbound          Dictionary<,>
    //     constructed
    //         open         Dictionary<TKey,TValue>
    //         closed       Dictionary<string,int>

    /// <summary>
    /// Attempts to lookup the ID type of the specified resource type. Returns <c>null</c> if it does not implement <see cref="IIdentifiable{TId}" />.
    /// </summary>
    public Type? LookupIdType(Type? resourceClrType)
    {
        Type? identifiableClosedInterface = resourceClrType?.GetInterfaces().FirstOrDefault(@interface =>
            @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IIdentifiable<>));

        return identifiableClosedInterface?.GetGenericArguments()[0];
    }

    /// <summary>
    /// Attempts to get a descriptor for the specified resource type.
    /// </summary>
    public ResourceDescriptor? ResolveResourceDescriptor(Type? type)
    {
        if (type != null && type.IsOrImplementsInterface<IIdentifiable>())
        {
            Type? idType = LookupIdType(type);

            if (idType != null)
            {
                return new ResourceDescriptor(type, idType);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the implementation type with service interface (to be registered in the IoC container) for the specified open interface and its type arguments,
    /// by scanning for types in the specified assembly that match the signature.
    /// </summary>
    /// <param name="assembly">
    /// The assembly to search for matching types.
    /// </param>
    /// <param name="openInterface">
    /// The open generic interface to match against.
    /// </param>
    /// <param name="interfaceTypeArguments">
    /// Generic type arguments to construct <paramref name="openInterface" />.
    /// </param>
    /// <example>
    /// <code><![CDATA[
    /// GetContainerRegistrationFromAssembly(assembly, typeof(IResourceService<,>), typeof(Article), typeof(Guid));
    /// ]]></code>
    /// </example>
    public (Type implementationType, Type serviceInterface)? GetContainerRegistrationFromAssembly(Assembly assembly, Type openInterface,
        params Type[] interfaceTypeArguments)
    {
        ArgumentGuard.NotNull(assembly, nameof(assembly));
        ArgumentGuard.NotNull(openInterface, nameof(openInterface));
        ArgumentGuard.NotNull(interfaceTypeArguments, nameof(interfaceTypeArguments));

        if (!openInterface.IsInterface || !openInterface.IsGenericType || openInterface != openInterface.GetGenericTypeDefinition())
        {
            throw new ArgumentException($"Specified type '{openInterface.FullName}' is not an open generic interface.", nameof(openInterface));
        }

        if (interfaceTypeArguments.Length != openInterface.GetGenericArguments().Length)
        {
            throw new ArgumentException(
                $"Interface '{openInterface.FullName}' requires {openInterface.GetGenericArguments().Length} type arguments " +
                $"instead of {interfaceTypeArguments.Length}.", nameof(interfaceTypeArguments));
        }

        return assembly.GetTypes().Select(type => GetContainerRegistrationFromType(type, openInterface, interfaceTypeArguments))
            .FirstOrDefault(result => result != null);
    }

    private static (Type implementationType, Type serviceInterface)? GetContainerRegistrationFromType(Type nextType, Type openInterface,
        Type[] interfaceTypeArguments)
    {
        if (!nextType.IsNested)
        {
            foreach (Type nextConstructedInterface in nextType.GetInterfaces().Where(type => type.IsGenericType))
            {
                Type nextOpenInterface = nextConstructedInterface.GetGenericTypeDefinition();

                if (nextOpenInterface == openInterface)
                {
                    Type[] nextTypeArguments = nextConstructedInterface.GetGenericArguments();

                    if (nextTypeArguments.Length == interfaceTypeArguments.Length && nextTypeArguments.SequenceEqual(interfaceTypeArguments))
                    {
                        return (nextType, nextOpenInterface.MakeGenericType(interfaceTypeArguments));
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Scans for types in the specified assembly that derive from the specified open type.
    /// </summary>
    /// <param name="assembly">
    /// The assembly to search for derived types.
    /// </param>
    /// <param name="openType">
    /// The open generic interface to match against.
    /// </param>
    /// <param name="typeArguments">
    /// Generic type arguments to construct <paramref name="openType" />.
    /// </param>
    /// <example>
    /// <code><![CDATA[
    /// GetDerivedTypesForOpenType(assembly, typeof(ResourceDefinition<,>), typeof(Article), typeof(int))
    /// ]]></code>
    /// </example>
    public IReadOnlyCollection<Type> GetDerivedTypesForOpenType(Assembly assembly, Type openType, params Type[] typeArguments)
    {
        ArgumentGuard.NotNull(assembly, nameof(assembly));
        ArgumentGuard.NotNull(openType, nameof(openType));
        ArgumentGuard.NotNull(typeArguments, nameof(typeArguments));

        Type closedType = openType.MakeGenericType(typeArguments);
        return GetDerivedTypes(assembly, closedType).ToArray();
    }

    /// <summary>
    /// Gets all derivatives of the specified type.
    /// </summary>
    /// <param name="assembly">
    /// The assembly to search.
    /// </param>
    /// <param name="baseType">
    /// The inherited type.
    /// </param>
    /// <example>
    /// <code>
    /// GetDerivedTypes(assembly, typeof(DbContext))
    /// </code>
    /// </example>
    public IEnumerable<Type> GetDerivedTypes(Assembly assembly, Type baseType)
    {
        ArgumentGuard.NotNull(assembly, nameof(assembly));
        ArgumentGuard.NotNull(baseType, nameof(baseType));

        foreach (Type type in assembly.GetTypes())
        {
            if (baseType.IsAssignableFrom(type))
            {
                yield return type;
            }
        }
    }
}
