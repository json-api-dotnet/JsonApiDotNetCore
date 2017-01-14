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
    }
}
