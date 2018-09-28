using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Abstract class to make available shared properties of all query implementations
    /// It elimines boilerplate of providing specified type(AttrQuery or RelatedAttrQuery) 
    /// while filter and sort operations and eliminates plenty of methods to keep DRY principles 
    /// </summary>
    public abstract class BaseAttrQuery
    {
        protected BaseAttrQuery(RelationshipAttribute relationship, AttrAttribute attr)
        {
            Relationship = relationship;
            Attr = attr;
        }

        public AttrAttribute Attr { get; }
        public RelationshipAttribute Relationship { get; }
        public bool IsAttributeOfRelationship => Relationship != null;

        public string GetPropertyPath()
        {
            if (IsAttributeOfRelationship)
                return string.Format("{0}.{1}", Relationship.InternalRelationshipName, Attr.InternalAttributeName);
            else
                return Attr.InternalAttributeName;
        }
    }
}
