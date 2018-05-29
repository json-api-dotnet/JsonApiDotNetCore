using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JsonApiDotNetCore.Internal
{
    public static class TypeHelper
    {
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

        /// <summary>
        /// Convert collection of query string params to Collection of concrete Type
        /// </summary>
        /// <param name="values">Collection like ["10","20","30"]</param>
        /// <param name="type">Non array type. For e.g. int</param>
        /// <returns>Collection of concrete type</returns>
        public static object ConvertListType(IEnumerable<string> values, Type type)
        {
            var convertedArray = new List<object>();
            foreach (var value in values)
            {
                convertedArray.Add(ConvertType(value, type));
            }
            var listType = typeof(List<>).MakeGenericType(type);
            IList list = (IList)Activator.CreateInstance(listType);
            foreach (var item in convertedArray)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
