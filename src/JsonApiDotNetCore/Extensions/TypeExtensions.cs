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
        public static IList CopyToList(this IEnumerable copyFrom, Type elementType, Converter<object, object> elementConverter = null)
        {
            Type collectionType = typeof(List<>).MakeGenericType(elementType);

            if (elementConverter != null)
            {
                var converted = copyFrom.Cast<object>().Select(element => elementConverter(element));
                return (IList) CopyToTypedCollection(converted, collectionType);
            }

            return (IList)CopyToTypedCollection(copyFrom, collectionType);
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
            dynamic concreteCollectionInstance = TypeHelper.CreateInstance(concreteCollectionType);

            foreach (var item in source)
            {
                concreteCollectionInstance.Add((dynamic) item);
            }

            return concreteCollectionInstance;
        }

        public static string GetResourceStringId<TResource, TId>(TId id) where TResource : class, IIdentifiable<TId>
        {
            var tempResource = TypeHelper.CreateInstance<TResource>();
            tempResource.Id = id;
            return tempResource.StringId;
        }

        /// <summary>
        /// Whether the specified source type implements or equals the specified interface.
        /// </summary>
        public static bool IsOrImplementsInterface(this Type source, Type interfaceType)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (source == null)
            {
                return false;
            }

            return source == interfaceType || source.GetInterfaces().Any(type => type == interfaceType);
        }

        public static bool HasSingleConstructorWithoutParameters(this Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();

            return constructors.Length == 1 && constructors[0].GetParameters().Length == 0;
        }
    }
}
