using System;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    public class AttrAttribute : Attribute
    {
        public AttrAttribute(string publicName, bool isImmutable = false, bool isFilterable = true, bool isSortable = true)
        {
            PublicAttributeName = publicName;
            IsImmutable = isImmutable;
            IsFilterable = isFilterable;
            IsSortable = isSortable;
        }

        internal AttrAttribute(string publicName, string internalName, bool isImmutable = false)
        {
            PublicAttributeName = publicName;
            InternalAttributeName = internalName;
            IsImmutable = isImmutable;
        }

        public string PublicAttributeName { get; }
        public string InternalAttributeName { get; }
        public bool IsImmutable { get; }
        public bool IsFilterable { get; }
        public bool IsSortable { get; }

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
