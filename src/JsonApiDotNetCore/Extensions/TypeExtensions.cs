using JsonApiDotNetCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Extensions
{
    internal static class TypeExtensions
    {
        public static Type GetElementType(this IEnumerable enumerable)
        {
            var enumerableTypes = enumerable.GetType()
                .GetInterfaces()
                .Where(t => t.IsGenericType == true && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .ToList();

            var numberOfEnumerableTypes = enumerableTypes.Count;

            if (numberOfEnumerableTypes == 0)
            {
                throw new ArgumentException($"{nameof(enumerable)} of type {enumerable.GetType().FullName} does not implement a generic variant of {nameof(IEnumerable)}");
            }

            if (numberOfEnumerableTypes > 1)
            {
                throw new ArgumentException($"{nameof(enumerable)} of type {enumerable.GetType().FullName} implements more than one generic variant of {nameof(IEnumerable)}:\n" +
                    $"{string.Join("\n", enumerableTypes.Select(t => t.FullName))}");
            }

            var elementType = enumerableTypes[0].GenericTypeArguments[0];

            return elementType;
        }

        /// <summary>
        /// Creates a List{TInterface} where TInterface is the generic for type specified by t
        /// </summary>
        public static IEnumerable GetEmptyCollection(this Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            var listType = typeof(List<>).MakeGenericType(t);
            var list = (IEnumerable)Activator.CreateInstance(listType);
            return list;
        }

        /// <summary>
        /// Creates a new instance of type t, casting it to the specified TInterface 
        /// </summary>
        public static TInterface New<TInterface>(this Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            var instance = (TInterface)CreateNewInstance(t);
            return instance;
        }

        private static object CreateNewInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new JsonApiException(500, $"Type '{type}' cannot be instantiated using the default constructor.", e);
            }
        }

        /// <summary>
        /// Whether or not a type implements an interface.
        /// </summary>
        public static bool Implements<T>(this Type concreteType) 
            => Implements(concreteType, typeof(T));

        /// <summary>
        /// Whether or not a type implements an interface.
        /// </summary>
        public static bool Implements(this Type concreteType, Type interfaceType) 
            => interfaceType?.IsAssignableFrom(concreteType) == true;
    }
}
