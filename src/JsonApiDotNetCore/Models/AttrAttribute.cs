using System;
using System.Reflection;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    public class AttrAttribute : Attribute
    {
        public AttrAttribute(string publicName)
        {
            PublicAttributeName = publicName;
        }

        public AttrAttribute(string publicName, string internalName)
        {
            PublicAttributeName = publicName;
            InternalAttributeName = internalName;
        }

        public string PublicAttributeName { get; set; }
        public string InternalAttributeName { get; set; }

        public object GetValue(object entity)
        {
            return entity
                .GetType()
                .GetProperty(InternalAttributeName)
                .GetValue(entity);
        }

        public void SetValue(object entity, object newValue)
        {
            var propertyInfo = entity
                .GetType()
                .GetProperty(InternalAttributeName);

            if (propertyInfo != null)
            {
                var convertedValue = TypeHelper.ConvertType(newValue, propertyInfo.PropertyType);

                propertyInfo.SetValue(entity, convertedValue);
            }
        }
    }
}
