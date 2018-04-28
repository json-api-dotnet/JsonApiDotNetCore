using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JsonApiDotNetCore.Internal
{
    public static class TypeHelper
    {
        public static IList ConvertCollection(IEnumerable<object> collection, Type targetType)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType)) as IList;
            foreach(var item in collection)
                list.Add(ConvertType(item, targetType));
            return list;
        }

        public static object ConvertType(object value, Type type)
        {
            if (value == null)
                return null;

            var valueType = value.GetType();

            try
            {
                if (valueType == type || type.IsAssignableFrom(valueType))
                    return value;

                type = Nullable.GetUnderlyingType(type) ?? type;

                var stringValue = value.ToString();

                if (string.IsNullOrEmpty(stringValue))
                    return GetDefaultType(type);

                if (type == typeof(Guid))
                    return Guid.Parse(stringValue);

                if (type == typeof(DateTimeOffset))
                    return DateTimeOffset.Parse(stringValue);

                if (type.GetTypeInfo().IsEnum)
                    return Enum.Parse(type, stringValue);

                return Convert.ChangeType(stringValue, type);
            }
            catch (Exception e)
            {
                throw new FormatException($"{ valueType } cannot be converted to { type }", e);
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
    }
}
