using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
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
        private readonly IJsonApiContext _jsonApiContext;

        public BaseAttrQuery(IJsonApiContext jsonApiContext, BaseQuery baseQuery)
        {
            _jsonApiContext = jsonApiContext;
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
            => _jsonApiContext.RequestEntity.Attributes.FirstOrDefault(attr => attr.Is(attribute));

        private RelationshipAttribute GetRelationship(string propertyName)
            => _jsonApiContext.RequestEntity.Relationships.FirstOrDefault(r => r.Is(propertyName));

        private AttrAttribute GetAttribute(RelationshipAttribute relationship, string attribute)
        {
            var relatedContextExntity = _jsonApiContext.ContextGraph.GetContextEntity(relationship.Type);
            return relatedContextExntity.Attributes
              .FirstOrDefault(a => a.Is(attribute));
        }
    }
}
