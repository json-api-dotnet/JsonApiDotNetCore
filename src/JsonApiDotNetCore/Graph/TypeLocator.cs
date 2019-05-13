using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    /// <summary>
    /// Used to locate types and facilitate auto-resource discovery
    /// </summary>
    internal static class TypeLocator
    {
        private static Dictionary<Assembly, Type[]> _typeCache = new Dictionary<Assembly, Type[]>();
        private static Dictionary<Assembly, List<ResourceDescriptor>> _identifiableTypeCache = new Dictionary<Assembly, List<ResourceDescriptor>>();


        /// <summary>
        /// Determine whether or not this is a json:api resource by checking if it implements <see cref="IIdentifiable"/>.
        /// Returns the status and the resultant id type, either `(true, Type)` OR `(false, null)`
        /// </summary>        
        public static (bool isJsonApiResource, Type idType) GetIdType(Type resourceType)
        {
            var identitifableType = GetIdentifiableIdType(resourceType);
            return (identitifableType != null)
                ? (true, identitifableType)
                : (false, null);
        }

        private static Type GetIdentifiableIdType(Type identifiableType)
            => GetIdentifiableInterface(identifiableType)?.GetGenericArguments()[0];

        private static Type GetIdentifiableInterface(Type type)
            => type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIdentifiable<>));

        // TODO: determine if this optimization is even helpful...
        private static Type[] GetAssemblyTypes(Assembly assembly)
        {
            if (_typeCache.TryGetValue(assembly, out var types) == false)
            {
                types = assembly.GetTypes();
                _typeCache[assembly] = types;
            }

            return types;
        }

        /// <summary>
        /// Get all implementations of <see cref="IIdentifiable"/> in the assembly
        /// </summary>
        public static IEnumerable<ResourceDescriptor> GetIdentifableTypes(Assembly assembly)
            => (_identifiableTypeCache.TryGetValue(assembly, out var descriptors) == false)
                    ? FindIdentifableTypes(assembly)
                    : _identifiableTypeCache[assembly];

        private static IEnumerable<ResourceDescriptor> FindIdentifableTypes(Assembly assembly)
        {
            var descriptors = new List<ResourceDescriptor>();
            _identifiableTypeCache[assembly] = descriptors;

            foreach (var type in assembly.GetTypes())
            {
                if (TryGetResourceDescriptor(type, out var descriptor))
                {
                    descriptors.Add(descriptor);
                    yield return descriptor;
                }
            }
        }

        /// <summary>
        /// Attempts to get a descriptor of the resource type.
        /// </summary>
        /// <returns>
        /// True if the type is a valid json:api type (must implement <see cref="IIdentifiable"/>), false otherwise.
        /// </returns>
        internal static bool TryGetResourceDescriptor(Type type, out ResourceDescriptor descriptor)
        {
            var possible = GetIdType(type);
            if (possible.isJsonApiResource)
            {
                descriptor = new ResourceDescriptor(type, possible.idType);
                return true;
            }

            descriptor = ResourceDescriptor.Empty;
            return false;
        }
        /// <summary>
        /// Get all implementations of the generic interface
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <param name="openGenericInterfaceType">The open generic type, e.g. `typeof(IResourceService&lt;&gt;)`</param>
        /// <param name="genericInterfaceArguments">Parameters to the generic type</param>
        /// <example>
        /// <code>
        /// GetGenericInterfaceImplementation(assembly, typeof(IResourceService&lt;&gt;), typeof(Article), typeof(Guid));
        /// </code>
        /// </example>
        public static (Type implementation, Type registrationInterface) GetGenericInterfaceImplementation(Assembly assembly, Type openGenericInterfaceType, params Type[] genericInterfaceArguments)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (openGenericInterfaceType == null) throw new ArgumentNullException(nameof(openGenericInterfaceType));
            if (genericInterfaceArguments == null) throw new ArgumentNullException(nameof(genericInterfaceArguments));
            if (genericInterfaceArguments.Length == 0) throw new ArgumentException("No arguments supplied for the generic interface.", nameof(genericInterfaceArguments));
            if (openGenericInterfaceType.IsGenericType == false) throw new ArgumentException("Requested type is not a generic type.", nameof(openGenericInterfaceType));

            foreach (var type in assembly.GetTypes())
            {
                var interfaces = type.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
                        if (interfaceType.GetGenericArguments().First() == genericInterfaceArguments.First() &&genericTypeDefinition == openGenericInterfaceType.GetGenericTypeDefinition())
                        {
                            return (
                                type,
                                genericTypeDefinition.MakeGenericType(genericInterfaceArguments)
                            );
                        }
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Get all derivitives of the concrete, generic type.
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <param name="openGenericType">The open generic type, e.g. `typeof(ResourceDefinition&lt;&gt;)`</param>
        /// <param name="genericArguments">Parameters to the generic type</param>
        /// <example>
        /// <code>
        /// GetDerivedGenericTypes(assembly, typeof(ResourceDefinition<>), typeof(Article))
        /// </code>
        /// </example>
        public static IEnumerable<Type> GetDerivedGenericTypes(Assembly assembly, Type openGenericType, params Type[] genericArguments)
        {
            var genericType = openGenericType.MakeGenericType(genericArguments);
            return GetDerivedTypes(assembly, genericType);
        }

        /// <summary>
        /// Get all derivitives of the specified type.
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <param name="openGenericType">The inherited type</param>
        /// <example>
        /// <code>
        /// GetDerivedGenericTypes(assembly, typeof(DbContext))
        /// </code>
        /// </example>
        public static IEnumerable<Type> GetDerivedTypes(Assembly assembly, Type inheritedType)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (inheritedType.IsAssignableFrom(type))
                    yield return type;
            }
        }
    }
}
