using System;
using System.Reflection;

namespace JsonApiDotNetCore.Internal
{
    public static class TypeHelper
    {
        public static object ConvertType(object value, Type type)
        {
            try
            {
                if (value == null)
                    return null;

                type = Nullable.GetUnderlyingType(type) ?? type;

                var stringValue = value.ToString();

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
                throw new FormatException($"{ value } cannot be converted to { type.GetTypeInfo().Name }", e);
            }
        }
    }
}
