using System;
using System.Reflection;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    public class AttrAttribute : Attribute
    {
        public AttrAttribute(string publicName, bool isImmutable = false)
        {
            PublicAttributeName = publicName;
            IsImmutable = isImmutable;
        }

        public AttrAttribute(string publicName, string internalName, bool isImmutable = false)
        {
            PublicAttributeName = publicName;
            InternalAttributeName = internalName;
            IsImmutable = isImmutable;
        }

        public string PublicAttributeName { get; set; }
        public string InternalAttributeName { get; set; }
        public bool IsImmutable { get; set; }

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
