using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to locate types and facilitate resource auto-discovery.
    /// </summary>
    internal sealed class TypeLocator
    {
        /// <summary>
        /// Attempts to lookup the ID type of the specified resource type. Returns <c>null</c> if it does not implement <see cref="IIdentifiable{TId}" />.
        /// </summary>
        public Type TryGetIdType(Type resourceType)
        {
            Type identifiableInterface = resourceType.GetInterfaces().FirstOrDefault(@interface =>
                @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IIdentifiable<>));

            return identifiableInterface?.GetGenericArguments()[0];
        }

        /// <summary>
        /// Attempts to get a descriptor for the specified resource type.
        /// </summary>
        public ResourceDescriptor TryGetResourceDescriptor(Type type)
        {
            if (type.IsOrImplementsInterface(typeof(IIdentifiable)))
            {
                Type idType = TryGetIdType(type);

                if (idType != null)
                {
                    return new ResourceDescriptor(type, idType);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all implementations of a generic interface.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to search in.
        /// </param>
        /// <param name="openGenericInterface">
        /// The open generic interface.
        /// </param>
        /// <param name="interfaceGenericTypeArguments">
        /// Generic type parameters to construct the generic interface.
        /// </param>
        /// <example>
        /// <code><![CDATA[
        /// GetGenericInterfaceImplementation(assembly, typeof(IResourceService<,>), typeof(Article), typeof(Guid));
        /// ]]></code>
        /// </example>
        public (Type implementation, Type registrationInterface)? GetGenericInterfaceImplementation(Assembly assembly, Type openGenericInterface,
            params Type[] interfaceGenericTypeArguments)
        {
            ArgumentGuard.NotNull(assembly, nameof(assembly));
            ArgumentGuard.NotNull(openGenericInterface, nameof(openGenericInterface));
            ArgumentGuard.NotNull(interfaceGenericTypeArguments, nameof(interfaceGenericTypeArguments));

            if (!openGenericInterface.IsInterface || !openGenericInterface.IsGenericType ||
                openGenericInterface != openGenericInterface.GetGenericTypeDefinition())
            {
                throw new ArgumentException($"Specified type '{openGenericInterface.FullName}' " + "is not an open generic interface.",
                    nameof(openGenericInterface));
            }

            if (interfaceGenericTypeArguments.Length != openGenericInterface.GetGenericArguments().Length)
            {
                throw new ArgumentException(
                    $"Interface '{openGenericInterface.FullName}' " + $"requires {openGenericInterface.GetGenericArguments().Length} type parameters " +
                    $"instead of {interfaceGenericTypeArguments.Length}.", nameof(interfaceGenericTypeArguments));
            }

            return assembly.GetTypes().Select(type => FindGenericInterfaceImplementationForType(type, openGenericInterface, interfaceGenericTypeArguments))
                .FirstOrDefault(result => result != null);
        }

        private static (Type implementation, Type registrationInterface)? FindGenericInterfaceImplementationForType(Type nextType, Type openGenericInterface,
            Type[] interfaceGenericTypeArguments)
        {
            foreach (Type nextGenericInterface in nextType.GetInterfaces().Where(type => type.IsGenericType))
            {
                Type nextOpenGenericInterface = nextGenericInterface.GetGenericTypeDefinition();

                if (nextOpenGenericInterface == openGenericInterface)
                {
                    Type[] nextGenericArguments = nextGenericInterface.GetGenericArguments();

                    if (nextGenericArguments.Length == interfaceGenericTypeArguments.Length &&
                        nextGenericArguments.SequenceEqual(interfaceGenericTypeArguments))
                    {
                        return (nextType, nextOpenGenericInterface.MakeGenericType(interfaceGenericTypeArguments));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all derivatives of the concrete, generic type.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to search.
        /// </param>
        /// <param name="openGenericType">
        /// The open generic type, e.g. `typeof(ResourceDefinition&lt;&gt;)`.
        /// </param>
        /// <param name="genericArguments">
        /// Parameters to the generic type.
        /// </param>
        /// <example>
        /// <code><![CDATA[
        /// GetDerivedGenericTypes(assembly, typeof(ResourceDefinition<>), typeof(Article))
        /// ]]></code>
        /// </example>
        public IReadOnlyCollection<Type> GetDerivedGenericTypes(Assembly assembly, Type openGenericType, params Type[] genericArguments)
        {
            Type genericType = openGenericType.MakeGenericType(genericArguments);
            return GetDerivedTypes(assembly, genericType).ToArray();
        }

        /// <summary>
        /// Gets all derivatives of the specified type.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to search.
        /// </param>
        /// <param name="inheritedType">
        /// The inherited type.
        /// </param>
        /// <example>
        /// <code>
        /// GetDerivedGenericTypes(assembly, typeof(DbContext))
        /// </code>
        /// </example>
        public IEnumerable<Type> GetDerivedTypes(Assembly assembly, Type inheritedType)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (inheritedType.IsAssignableFrom(type))
                {
                    yield return type;
                }
            }
        }
    }
}
