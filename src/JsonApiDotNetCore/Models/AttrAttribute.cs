using System;
using System.Reflection;

namespace JsonApiDotNetCore.Models
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

			if (propertyInfo != null)
			{
				Type t = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

				var convertedValue = (newValue == null) ? null : Convert.ChangeType(newValue, t);

				propertyInfo.SetValue(entity, convertedValue, null);
			}
        }
    }
}
