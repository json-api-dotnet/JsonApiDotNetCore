using System;
using System.Reflection;

namespace JsonApiDotNetCore.Internal
{
    public static class TypeHelper
    {
        public static object ConvertType(object value, Type type)
        {
            if(value == null)
                return null;

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                type = Nullable.GetUnderlyingType(type);            

            var stringValue = value.ToString();
            
            if(type == typeof(Guid))
                return Guid.Parse(stringValue);

            return Convert.ChangeType(stringValue, type);
        }
    }
}
