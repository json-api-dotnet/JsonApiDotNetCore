using System;

namespace JsonApiDotNetCore.Internal
{
    public static class TypeHelper
    {
        public static object ConvertType(object value, Type type)
        {
            if(value == null)
                return null;

           type = Nullable.GetUnderlyingType(type) ?? type;

            var stringValue = value.ToString();
            
            if(type == typeof(Guid))
                return Guid.Parse(stringValue);

            return Convert.ChangeType(stringValue, type);
        }
    }
}
