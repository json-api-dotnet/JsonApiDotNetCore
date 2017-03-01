using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Internal
{
    public class Relationship
    {
        public Type Type { get; set; }
        public Type BaseType { get {
            return (Type.GetInterfaces().Contains(typeof(IEnumerable))) ?
                 Type.GenericTypeArguments[0] :
                 Type;            
        } }

        public string RelationshipName { get; set; }

        public void SetValue(object entity, object newValue)
        {
            var propertyInfo = entity
                .GetType()
                .GetProperty(RelationshipName);
            
            propertyInfo.SetValue(entity, newValue);        
        }
    }
}
