using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
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
        private readonly IRequestManager _requestManager;
        private readonly IResourceGraph _resourceGraph;

        public BaseAttrQuery(IRequestManager requestManager, IResourceGraph resourceGraph, BaseQuery baseQuery)
        {
            _requestManager = requestManager ?? throw new ArgumentNullException(nameof(requestManager));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
            
            if(_resourceGraph == null) 
                throw new ArgumentException($"{nameof(IJsonApiContext)}.{nameof(_resourceGraph)} cannot be null. "
                    + "If this is a unit test, you need to construct a graph containing the resources being tested. "
                    + "See this issue to check the current status of improved test guidelines: "
                    + "https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/251", nameof(requestManager));
            
            if (baseQuery.IsAttributeOfRelationship)
            {
                Relationship = GetRelationship(baseQuery.Relationship);
                Attribute = GetAttribute(Relationship, baseQuery.Attribute);
            }
            else
            {
                Attribute = GetAttribute(baseQuery.Attribute);
            }
            
        }

        public AttrAttribute Attribute { get; }
        public RelationshipAttribute Relationship { get; }
        public bool IsAttributeOfRelationship => Relationship != null;

        public string GetPropertyPath()
        {
            if (IsAttributeOfRelationship)
                return string.Format("{0}.{1}", Relationship.InternalRelationshipName, Attribute.InternalAttributeName);
            else
                return Attribute.InternalAttributeName;
        }

        private AttrAttribute GetAttribute(string attribute)
        {
            return _requestManager.GetRequestResource().Attributes.FirstOrDefault(attr => attr.Is(attribute));
        }

        private RelationshipAttribute GetRelationship(string propertyName)
        {
            return _requestManager.GetRequestResource().Relationships.FirstOrDefault(r => r.Is(propertyName));
        }

        private AttrAttribute GetAttribute(RelationshipAttribute relationship, string attribute)
        {
            var relatedContextEntity = _resourceGraph.GetContextEntity(relationship.DependentType);
            return relatedContextEntity.Attributes
              .FirstOrDefault(a => a.Is(attribute));
        }
    }
}
