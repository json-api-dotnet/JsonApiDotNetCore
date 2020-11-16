using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore
{
    internal static class TypeHelper
    {
        private static readonly Type[] _hashSetCompatibleCollectionTypes =
        {
            typeof(HashSet<>), typeof(ICollection<>), typeof(ISet<>), typeof(IEnumerable<>), typeof(IReadOnlyCollection<>)
        };

        public static object ConvertType(object value, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (value == null)
            {
                if (!CanContainNull(type))
                {
                    throw new FormatException($"Failed to convert 'null' to type '{type.Name}'.");
                }

                return null;
            }

            Type runtimeType = value.GetType();
            if (type == runtimeType || type.IsAssignableFrom(runtimeType))
            {
                return value;
            }

            string stringValue = value.ToString();
            if (string.IsNullOrEmpty(stringValue))
            {
                return GetDefaultValue(type);
            }

            bool isNullableTypeRequested = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

            try
            {
                if (nonNullableType == typeof(Guid))
                {
                    Guid convertedValue = Guid.Parse(stringValue);
                    return isNullableTypeRequested ? (Guid?) convertedValue : convertedValue;
                }

                if (nonNullableType == typeof(DateTimeOffset))
                {
                    DateTimeOffset convertedValue = DateTimeOffset.Parse(stringValue);
                    return isNullableTypeRequested ? (DateTimeOffset?) convertedValue : convertedValue;
                }

                if (nonNullableType == typeof(TimeSpan))
                {
                    TimeSpan convertedValue = TimeSpan.Parse(stringValue);
                    return isNullableTypeRequested ? (TimeSpan?) convertedValue : convertedValue;
                }

                if (nonNullableType.IsEnum)
                {
                    object convertedValue = Enum.Parse(nonNullableType, stringValue);

                    // https://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
                    return convertedValue;
                }

                // https://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
                return Convert.ChangeType(stringValue, nonNullableType);
            }
            catch (Exception exception) when (exception is FormatException || exception is OverflowException ||
                                              exception is InvalidCastException || exception is ArgumentException)
            {
                throw new FormatException(
                    $"Failed to convert '{value}' of type '{runtimeType.Name}' to type '{type.Name}'.", exception);
            }
        }

        public static bool CanContainNull(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? CreateInstance(type) : null;
        }

        public static Type TryGetCollectionElementType(Type type)
        {
            if (type != null)
            {
                if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
                {
                    if (IsOrImplementsInterface(type, typeof(IEnumerable)))
                    {
                        return type.GenericTypeArguments[0];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the property info that is referenced in the NavigationAction expression.
        /// Credits: https://stackoverflow.com/a/17116267/4441216
        /// </summary>
        public static PropertyInfo ParseNavigationExpression<TResource>(Expression<Func<TResource, object>> navigationExpression)
        {
            MemberExpression exp;

            //this line is necessary, because sometimes the expression comes in as Convert(originalExpression)
            if (navigationExpression.Body is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand is MemberExpression memberExpression)
                {
                    exp = memberExpression;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (navigationExpression.Body is MemberExpression memberExpression)
            {
                exp = memberExpression;
            }
            else
            {
                throw new ArgumentException();
            }

            return (PropertyInfo)exp.Member;
        }

        /// <summary>
        /// Creates an instance of the specified generic type
        /// </summary>
        /// <returns>The instance of the parameterized generic type</returns>
        /// <param name="parameters">Generic type parameters to be used in open type.</param>
        /// <param name="constructorArguments">Constructor arguments to be provided in instantiation.</param>
        /// <param name="openType">Open generic type</param>
        private static object CreateInstanceOfOpenType(Type openType, Type[] parameters, params object[] constructorArguments)
        {
            var parameterizedType = openType.MakeGenericType(parameters);
            return Activator.CreateInstance(parameterizedType, constructorArguments);
        }

        /// <summary>
        /// Helper method that "unboxes" the TValue from the relationship dictionary into  
        /// </summary>
        public static Dictionary<RelationshipAttribute, HashSet<TValueOut>> ConvertRelationshipDictionary<TValueOut>(Dictionary<RelationshipAttribute, IEnumerable> relationships)
        {
            return relationships.ToDictionary(pair => pair.Key, pair => (HashSet<TValueOut>)pair.Value);
        }

        /// <summary>
        /// Converts a dictionary of AttrAttributes to the underlying PropertyInfo that is referenced
        /// </summary>
        public static Dictionary<PropertyInfo, HashSet<TValueOut>> ConvertAttributeDictionary<TValueOut>(IEnumerable<AttrAttribute> attributes, HashSet<TValueOut> resources)
        {
            return attributes?.ToDictionary(attr => attr.Property, attr => resources);
        }

        /// <summary>
        /// Creates an instance of the specified generic type
        /// </summary>
        /// <returns>The instance of the parameterized generic type</returns>
        /// <param name="parameter">Generic type parameter to be used in open type.</param>
        /// <param name="constructorArguments">Constructor arguments to be provided in instantiation.</param>
        /// <param name="openType">Open generic type</param>
        public static object CreateInstanceOfOpenType(Type openType, Type parameter, params object[] constructorArguments)
        {
            return CreateInstanceOfOpenType(openType, new[] {parameter}, constructorArguments);
        }

        /// <summary>
        /// Use this overload if you need to instantiate a type that has a internal constructor
        /// </summary>
        public static object CreateInstanceOfOpenType(Type openType, Type parameter, bool hasInternalConstructor, params object[] constructorArguments)
        {
            Type[] parameters = {parameter};
            if (!hasInternalConstructor) return CreateInstanceOfOpenType(openType, parameters, constructorArguments);
            var parameterizedType = openType.MakeGenericType(parameters);
            // note that if for whatever reason the constructor of AffectedResource is set from
            // internal to public, this will throw an error, as it is looking for a no
            return Activator.CreateInstance(parameterizedType, BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArguments, null);
        }

        /// <summary>
        /// Reflectively instantiates a list of a certain type.
        /// </summary>
        /// <returns>The list of the target type</returns>
        /// <param name="type">The target type</param>
        public static IList CreateListFor(Type type)
        {
            return (IList)CreateInstanceOfOpenType(typeof(List<>), type);
        }

        /// <summary>
        /// Reflectively instantiates a hashset of a certain type. 
        /// </summary>
        public static IEnumerable CreateHashSetFor(Type type, object elements = null)
        {
            return (IEnumerable)CreateInstanceOfOpenType(typeof(HashSet<>), type, elements ?? new object());
        }

        /// <summary>
        /// Returns a compatible collection type that can be instantiated, for example IList{Article} -> List{Article} or ISet{Article} -> HashSet{Article}
        /// </summary>
        public static Type ToConcreteCollectionType(Type collectionType)
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
        /// Indicates whether a <see cref="HashSet{T}"/> instance can be assigned to the specified type,
        /// for example IList{Article} -> false or ISet{Article} -> true.
        /// </summary>
        public static bool TypeCanContainHashSet(Type collectionType)
        {
            if (collectionType.IsGenericType)
            {
                var openCollectionType = collectionType.GetGenericTypeDefinition();
                return _hashSetCompatibleCollectionTypes.Contains(openCollectionType);
            }

            return false;
        }

        /// <summary>
        /// Gets the type (Guid or int) of the Id of a type that implements IIdentifiable
        /// </summary>
        public static Type GetIdType(Type resourceType)
        {
            var property = resourceType.GetProperty(nameof(Identifiable.Id));
            if (property == null) throw new ArgumentException("Type does not have 'Id' property.");
            return property.PropertyType;
        }

        public static ICollection<IIdentifiable> ExtractResources(object value)
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
                return new[] {resource};
            }

            return Array.Empty<IIdentifiable>();
        }

        public static object CreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of '{type.FullName}' using its default constructor.", exception);
            }
        }

        /// <summary>
        /// Extension to use the LINQ cast method in a non-generic way:
        /// <code>
        /// Type targetType = typeof(TResource)
        /// ((IList)myList).CopyToList(targetType).
        /// </code>
        /// </summary>
        public static IList CopyToList(IEnumerable copyFrom, Type elementType, Converter<object, object> elementConverter = null)
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
        public static IEnumerable CopyToTypedCollection(IEnumerable source, Type collectionType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (collectionType == null) throw new ArgumentNullException(nameof(collectionType));

            var concreteCollectionType = ToConcreteCollectionType(collectionType);
            dynamic concreteCollectionInstance = CreateInstance(concreteCollectionType);

            foreach (var item in source)
            {
                concreteCollectionInstance.Add((dynamic) item);
            }

            return concreteCollectionInstance;
        }

        /// <summary>
        /// Whether the specified source type implements or equals the specified interface.
        /// </summary>
        public static bool IsOrImplementsInterface(Type source, Type interfaceType)
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
    }
}
