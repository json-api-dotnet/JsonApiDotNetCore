using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    internal static class TypeLocator
    {
        private static Dictionary<Assembly, Type[]> _typeCache = new Dictionary<Assembly, Type[]>();
        private static Dictionary<Assembly, List<ResourceDescriptor>> _identifiableTypeCache = new Dictionary<Assembly, List<ResourceDescriptor>>();
        
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

        public static List<ResourceDescriptor> GetIdentifableTypes(Assembly assembly)
        {
            if (_identifiableTypeCache.TryGetValue(assembly, out var descriptors) == false)
            {
                descriptors = new List<ResourceDescriptor>();
                _identifiableTypeCache[assembly] = descriptors;

                foreach (var type in assembly.GetTypes())
                {
                    var possible = GetIdType(type);
                    if (possible.isJsonApiResource)
                        descriptors.Add(new ResourceDescriptor(type, possible.idType));
                }
            }

            return descriptors;
        }

        public static (Type implementation, Type registrationInterface) GetGenericInterfaceImplementation(Assembly assembly, Type openGenericInterfaceType, params Type[] genericInterfaceArguments)
        {
            foreach (var type in assembly.GetTypes())
            {
                var interfaces = type.GetInterfaces();
                foreach (var interfaceType in interfaces)
                    if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == openGenericInterfaceType)
                        return (
                            type,
                            interfaceType.MakeGenericType(genericInterfaceArguments)
                        );
            }

            return (null, null);
        }

        public static IEnumerable<Type> GetDerivedGenericTypes(Assembly assembly, Type openGenericType, Type genericArgument)
        {
            var genericType = openGenericType.MakeGenericType(genericArgument);
            foreach (var type in assembly.GetTypes())
            {
                if (genericType.IsAssignableFrom(type))
                    yield return type;
            }
        }

        public static IEnumerable<Type> GetDerivedTypes(Assembly assembly, Type inheritedType)
        {
            foreach (var type in assembly.GetTypes())
            {
                if(inheritedType.IsAssignableFrom(type))
                    yield return type;
            }
        }
    }
}
