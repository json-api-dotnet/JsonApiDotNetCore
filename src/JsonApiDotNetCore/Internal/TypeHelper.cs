using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    internal static class TypeHelper
    {
        public static object ConvertType(object value, Type type)
        {
            if (value == null && !CanBeNull(type))
                throw new FormatException("Cannot convert null to a non-nullable type");

            if (value == null)
                return null;

            Type runtimeType = value.GetType();

            try
            {
                if (runtimeType == type || type.IsAssignableFrom(runtimeType))
                    return value;

                type = Nullable.GetUnderlyingType(type) ?? type;

                var stringValue = value.ToString();

                if (string.IsNullOrEmpty(stringValue))
                    return GetDefaultValue(type);

                if (type == typeof(Guid))
                    return Guid.Parse(stringValue);

                if (type == typeof(DateTimeOffset))
                    return DateTimeOffset.Parse(stringValue);

                if (type == typeof(TimeSpan))
                    return TimeSpan.Parse(stringValue);

                if (type.IsEnum)
                    return Enum.Parse(type, stringValue);

                return Convert.ChangeType(stringValue, type);
            }
            catch (Exception exception)
            {
                throw new FormatException($"Failed to convert '{value}' of type '{runtimeType}' to type '{type}'.", exception);
            }
        }

        private static bool CanBeNull(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        internal static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? CreateInstance(type) : null;
        }

        public static Type TryGetCollectionElementType(Type type)
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
        /// Convert collection of query string params to Collection of concrete Type
        /// </summary>
        /// <param name="values">Collection like ["10","20","30"]</param>
        /// <param name="type">Non array type. For e.g. int</param>
        /// <returns>Collection of concrete type</returns>
        public static IList ConvertListType(IEnumerable<string> values, Type type)
        {
            var list = CreateListFor(type);
            foreach (var value in values)
            {
                list.Add(ConvertType(value, type));
            }

            return list;
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
        /// <param name="attributes"></param>
        /// <param name="entities"></param>
        public static Dictionary<PropertyInfo, HashSet<TValueOut>> ConvertAttributeDictionary<TValueOut>(List<AttrAttribute> attributes, HashSet<TValueOut> entities)
        {
            return attributes?.ToDictionary(attr => attr.PropertyInfo, attr => entities);
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
            return CreateInstanceOfOpenType(openType, new[] { parameter }, constructorArguments);
        }

        /// <summary>
        /// Use this overload if you need to instantiate a type that has a internal constructor
        /// </summary>
        public static object CreateInstanceOfOpenType(Type openType, Type parameter, bool hasInternalConstructor, params object[] constructorArguments)
        {
            Type[] parameters = { parameter };
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
            IList list = (IList)CreateInstanceOfOpenType(typeof(List<>), type);
            return list;
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
        public static Type ToConcreteCollectionType(this Type collectionType)
        {
            if (collectionType.IsInterface && collectionType.IsGenericType)
            {
                var genericTypeDefinition = collectionType.GetGenericTypeDefinition();

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
        /// Gets the type (Guid or int) of the Id of a type that implements IIdentifiable
        /// </summary>
        public static Type GetIdType(Type resourceType)
        {
            var property = resourceType.GetProperty(nameof(Identifiable.Id));
            if (property == null) throw new ArgumentException("Type does not have 'Id' property.");
            return property.PropertyType;
        }

        public static T CreateInstance<T>()
        {
            return (T) CreateInstance(typeof(T));
        }

        public static object CreateInstance(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to create an instance of '{type.FullName}' using its default constructor.", exception);
            }
        }
    }
}
