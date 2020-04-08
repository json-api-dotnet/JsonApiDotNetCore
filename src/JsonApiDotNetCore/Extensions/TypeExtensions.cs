using JsonApiDotNetCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Extension to use the LINQ cast method in a non-generic way:
        /// <code>
        /// Type targetType = typeof(TResource)
        /// ((IList)myList).Cast(targetType).
        /// </code>
        /// </summary>
        public static IEnumerable Cast(this IEnumerable source, Type type)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (type == null) throw new ArgumentNullException(nameof(type));

            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            foreach (var item in source.Cast<object>())
            {
                list.Add(TypeHelper.ConvertType(item, type));
            }
            return list;
        }

        /// <summary>
        /// Creates a List{TInterface} where TInterface is the generic for type specified by t
        /// </summary>
        public static IEnumerable GetEmptyCollection(this Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            var listType = typeof(List<>).MakeGenericType(t);
            var list = (IEnumerable)CreateNewInstance(listType);
            return list;
        }

        public static string GetResourceStringId<TResource, TId>(TId id) where TResource : class, IIdentifiable<TId>
        {
            var tempResource = typeof(TResource).New<TResource>();
            tempResource.Id = id;
            return tempResource.StringId;
        }

        public static object New(this Type t)
        {
            return New<object>(t);
        }

        /// <summary>
        /// Creates a new instance of type t, casting it to the specified type.
        /// </summary>
        public static T New<T>(this Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            var instance = (T)CreateNewInstance(t);
            return instance;
        }

        private static object CreateNewInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to create an instance of '{type.FullName}' using its default constructor.", exception);
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
        private static bool Implements(this Type concreteType, Type interfaceType) 
            => interfaceType?.IsAssignableFrom(concreteType) == true;

        /// <summary>
        /// Whether or not a type inherits a base type.
        /// </summary>
        public static bool Inherits<T>(this Type concreteType) 
            => Inherits(concreteType, typeof(T));

        /// <summary>
        /// Whether or not a type inherits a base type.
        /// </summary>
        public static bool Inherits(this Type concreteType, Type interfaceType) 
            => interfaceType?.IsAssignableFrom(concreteType) == true;
    }
}
