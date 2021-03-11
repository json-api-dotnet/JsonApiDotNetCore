using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore
{
    internal class CollectionConverter
    {
        private static readonly Type[] HashSetCompatibleCollectionTypes =
        {
            typeof(HashSet<>),
            typeof(ICollection<>),
            typeof(ISet<>),
            typeof(IEnumerable<>),
            typeof(IReadOnlyCollection<>)
        };

        /// <summary>
        /// Creates a collection instance based on the specified collection type and copies the specified elements into it.
        /// </summary>
        /// <param name="source">
        /// Source to copy from.
        /// </param>
        /// <param name="collectionType">
        /// Target collection type, for example: typeof(List{Article}) or typeof(ISet{Person}).
        /// </param>
        public IEnumerable CopyToTypedCollection(IEnumerable source, Type collectionType)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(collectionType, nameof(collectionType));

            Type concreteCollectionType = ToConcreteCollectionType(collectionType);
            dynamic concreteCollectionInstance = Activator.CreateInstance(concreteCollectionType);

            foreach (object item in source)
            {
                concreteCollectionInstance!.Add((dynamic)item);
            }

            return concreteCollectionInstance;
        }

        /// <summary>
        /// Returns a compatible collection type that can be instantiated, for example IList{Article} -> List{Article} or ISet{Article} -> HashSet{Article}
        /// </summary>
        public Type ToConcreteCollectionType(Type collectionType)
        {
            if (collectionType.IsInterface && collectionType.IsGenericType)
            {
                Type genericTypeDefinition = collectionType.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(ISet<>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) || genericTypeDefinition == typeof(IReadOnlyCollection<>))
                {
                    return typeof(HashSet<>).MakeGenericType(collectionType.GenericTypeArguments[0]);
                }

                if (genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(IReadOnlyList<>))
                {
                    return typeof(List<>).MakeGenericType(collectionType.GenericTypeArguments[0]);
                }
            }

            return collectionType;
        }

        /// <summary>
        /// Returns a collection that contains zero, one or multiple resources, depending on the specified value.
        /// </summary>
        public ICollection<IIdentifiable> ExtractResources(object value)
        {
            if (value is ICollection<IIdentifiable> resourceCollection)
            {
                return resourceCollection;
            }

            if (value is IEnumerable<IIdentifiable> resources)
            {
                return resources.ToList();
            }

            if (value is IIdentifiable resource)
            {
                return resource.AsArray();
            }

            return Array.Empty<IIdentifiable>();
        }

        /// <summary>
        /// Returns the element type if the specified type is a generic collection, for example: IList{string} -> string or IList -> null.
        /// </summary>
        public Type TryGetCollectionElementType(Type type)
        {
            if (type != null)
            {
                if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
                {
                    if (type.IsOrImplementsInterface(typeof(IEnumerable)))
                    {
                        return type.GenericTypeArguments[0];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Indicates whether a <see cref="HashSet{T}" /> instance can be assigned to the specified type, for example IList{Article} -> false or ISet{Article} ->
        /// true.
        /// </summary>
        public bool TypeCanContainHashSet(Type collectionType)
        {
            if (collectionType.IsGenericType)
            {
                Type openCollectionType = collectionType.GetGenericTypeDefinition();
                return HashSetCompatibleCollectionTypes.Contains(openCollectionType);
            }

            return false;
        }
    }
}
