using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public abstract class BaseQuery
    {
        public BaseQuery(string attribute)
        {
            var properties = attribute.Split(QueryConstants.DOT);
            if(properties.Length > 1)
            {
                Relationship = properties[0];
                Attribute = properties[1];
            }
            else
                Attribute = properties[0];
        }

        public string Attribute { get; }
        public string Relationship { get; }
        public bool IsAttributeOfRelationship => Relationship != null;
    }
}
