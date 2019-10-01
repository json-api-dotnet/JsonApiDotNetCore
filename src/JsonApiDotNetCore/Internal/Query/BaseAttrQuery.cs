using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
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
        private readonly IContextEntityProvider _provider;
        private readonly ContextEntity _requestResource;

        public BaseAttrQuery(ContextEntity requestResource, IContextEntityProvider provider, BaseQuery baseQuery)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _requestResource = requestResource ?? throw new ArgumentNullException(nameof(requestResource));
            
            
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
            return _requestResource.Attributes.FirstOrDefault(attr => attr.Is(attribute));
        }

        private RelationshipAttribute GetRelationship(string propertyName)
        {
            return _requestResource.Relationships.FirstOrDefault(r => r.Is(propertyName));
        }

        private AttrAttribute GetAttribute(RelationshipAttribute relationship, string attribute)
        {
            var relatedContextEntity = _provider.GetContextEntity(relationship.DependentType);
            return relatedContextEntity.Attributes
              .FirstOrDefault(a => a.Is(attribute));
        }
    }
}
