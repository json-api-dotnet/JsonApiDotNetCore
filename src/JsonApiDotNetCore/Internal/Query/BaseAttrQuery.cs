using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    /// <summary>
    /// Abstract class to make available shared properties for AttrQuery and RelatedAttrQuery
    /// It elimines boilerplate of providing specified type(AttrQuery or RelatedAttrQuery) 
    /// while filter and sort operations and eliminates plenty of methods to keep DRY principles 
    /// </summary>
    public abstract class BaseAttrQuery
    {
        public AttrAttribute Attribute { get; protected set; }
        public RelationshipAttribute RelationshipAttribute { get; protected set; }
        public bool IsAttributeOfRelationship { get; protected set; }

        // Filter properties
        public string PropertyValue { get; protected set; }
        public FilterOperations FilterOperation { get; protected set; }
        // Sort properties
        public SortDirection Direction { get; protected set; }
    }
}
