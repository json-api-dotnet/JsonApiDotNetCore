using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    internal static class TypeHelper
    {
        public static IList ConvertCollection(IEnumerable<object> collection, Type targetType)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType)) as IList;
            foreach (var item in collection)
                list.Add(ConvertType(item, targetType));
            return list;
        }
        public static bool IsNullable(Type type)
        {
            return (!type.IsValueType || Nullable.GetUnderlyingType(type) != null);
        }
        public static object ConvertType(object value, Type type)
        {
            if (value == null && !IsNullable(type))
                throw new FormatException($"Cannot convert null to a non-nullable type");

            if (value == null)
                return null;

            Type typeOfValue = value.GetType();

            try
            {
                if (typeOfValue == type || type.IsAssignableFrom(typeOfValue))
                    return value;

                type = Nullable.GetUnderlyingType(type) ?? type;

                var stringValue = value?.ToString();

                if (string.IsNullOrEmpty(stringValue))
                    return GetDefaultType(type);

                if (type == typeof(Guid))
                    return Guid.Parse(stringValue);

                if (type == typeof(DateTimeOffset))
                    return DateTimeOffset.Parse(stringValue);


                if (type == typeof(TimeSpan))
                    return TimeSpan.Parse(stringValue);

                if (type.GetTypeInfo().IsEnum)
                    return Enum.Parse(type, stringValue);

                return Convert.ChangeType(stringValue, type);
            }
            catch (Exception e)
            {
                throw new FormatException($"{ typeOfValue } cannot be converted to { type }", e);
            }
        }

        private static object GetDefaultType(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static T ConvertType<T>(object value)
        {
            return (T)ConvertType(value, typeof(T));
        }

        public static Type GetTypeOfList(Type type)
        {
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        /// Gets the property info that is referenced in the NavigationAction expression.
        /// Credits: https://stackoverflow.com/a/17116267/4441216
        /// </summary>
        public static PropertyInfo ParseNavigationExpression<TResource>(Expression<Func<TResource, object>> NavigationExpression)
        {
            MemberExpression Exp = null;

            //this line is necessary, because sometimes the expression comes in as Convert(originalExpression)
            if (NavigationExpression.Body is UnaryExpression)
            {
                var UnExp = (UnaryExpression)NavigationExpression.Body;
                if (UnExp.Operand is MemberExpression)
                {
                    Exp = (MemberExpression)UnExp.Operand;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (NavigationExpression.Body is MemberExpression)
            {
                Exp = (MemberExpression)NavigationExpression.Body;
            }
            else
            {
                throw new ArgumentException();
            }

            return (PropertyInfo)Exp.Member;
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
        public static object CreateInstanceOfOpenType(Type openType, Type[] parameters, params object[] constructorArguments)
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
        /// Use this overload if you need to instantiate a type that has a internal constructor
        /// </summary>
        public static object CreateInstanceOfOpenType(Type openType, Type[] parameters, bool hasInternalConstructor, params object[] constructorArguments)
        {
            if (!hasInternalConstructor) return CreateInstanceOfOpenType(openType, parameters, constructorArguments);
            var parameterizedType = openType.MakeGenericType(parameters);
            // note that if for whatever reason the constructor of AffectedResource is set from
            // internal to public, this will throw an error, as it is looking for a no
            return Activator.CreateInstance(parameterizedType, BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArguments, null);
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
            return CreateInstanceOfOpenType(openType, new[] { parameter }, hasInternalConstructor, constructorArguments);

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
        /// Gets the generic argument T of List{T}
        /// </summary>
        /// <returns>The type of the list</returns>
        /// <param name="list">The list to be inspected</param>
        public static Type GetListInnerType(IEnumerable list)
        {
            return list.GetType().GetGenericArguments()[0];
        }

        /// <summary>
        /// Gets the type (Guid or int) of the Id of a type that implements IIdentifiable
        /// </summary>
        public static Type GetIdentifierType(Type entityType)
        {
            var property = entityType.GetProperty("Id");
            if (property == null) throw new ArgumentException("Type does not have a property Id");
            return entityType.GetProperty("Id").PropertyType;
        }

        /// <summary>
        /// Gets the type (Guid or int) of the Id of a type that implements IIdentifiable
        /// </summary>
        public static Type GetIdentifierType<T>() where T : IIdentifiable
        {
            return typeof(T).GetProperty("Id").PropertyType;
        }
    }
}
