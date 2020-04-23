using JsonApiDotNetCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Extension to use the LINQ cast method in a non-generic way:
        /// <code>
        /// Type targetType = typeof(TResource)
        /// ((IList)myList).CopyToList(targetType).
        /// </code>
        /// </summary>
        public static IEnumerable CopyToList(this IEnumerable source, Type type)
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
        /// Creates a collection instance based on the specified collection type and copies the specified elements into it.
        /// </summary>
        /// <param name="source">Source to copy from.</param>
        /// <param name="collectionType">Target collection type, for example: typeof(List{Article}) or typeof(ISet{Person}).</param>
        /// <returns></returns>
        public static IEnumerable CopyToTypedCollection(this IEnumerable source, Type collectionType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (collectionType == null) throw new ArgumentNullException(nameof(collectionType));

            var concreteCollectionType = collectionType.ToConcreteCollectionType();
            dynamic concreteCollectionInstance = concreteCollectionType.New<dynamic>();

            foreach (var item in source)
            {
                concreteCollectionInstance.Add((dynamic) item);
            }

            return concreteCollectionInstance;
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
        public static bool Implements(this Type concreteType, Type interfaceType) 
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

        public static bool ImplementsInterface(this Type source, Type interfaceType)
        {
            return source.GetInterfaces().Any(type => type == interfaceType);
        }
    }
}
