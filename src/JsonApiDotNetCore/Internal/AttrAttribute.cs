using System;
using System.Reflection;

namespace JsonApiDotNetCore.Internal
{
    public class AttrAttribute : Attribute
    {
        public AttrAttribute(string publicName)
        {
            PublicAttributeName = publicName;
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
                
            var convertedValue = Convert.ChangeType(newValue, propertyInfo.PropertyType);
            
            propertyInfo.SetValue(entity, convertedValue);
        }
    }
}
