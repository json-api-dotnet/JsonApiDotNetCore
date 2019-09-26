using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public abstract class ResourceObjectBuilder
    {
        protected readonly IResourceGraph _resourceGraph;
        protected readonly IContextEntityProvider _provider;
        private const string _identifiablePropertyName = nameof(Identifiable.Id);

        protected ResourceObjectBuilder(IResourceGraph resourceGraph, IContextEntityProvider provider)
        {
            _resourceGraph = resourceGraph;
            _provider = provider;
        }

        protected ResourceObject BuildResourceObject(IIdentifiable entity, IEnumerable<AttrAttribute> attrs = null, IEnumerable<RelationshipAttribute> rels = null)
        {
            var resourceContext = _provider.GetContextEntity(entity.GetType());

            // populating the top-level "type" and "id" members.
            var ro = new ResourceObject { Type = resourceContext.EntityName, Id = entity.StringId.NullIfEmpty() };

            // populating the top-level "attribute" member, if any
            if (attrs != null)
            {
                // never include "id" as an attribute
                attrs = attrs.Where(attr => attr.InternalAttributeName != _identifiablePropertyName);
                if (attrs.Any())
                {
                    ro.Attributes = new Dictionary<string, object>();
                    foreach (var attr in attrs)
                        ro.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
                }
            }

            // populating top-level "relationship" member, if any
            if (rels != null && rels.Any())
            {
                foreach (var rel in rels)
                {
                    var relData = GetRelationshipData(rel, entity);
                    if (relData != null)
                        (ro.Relationships = ro.Relationships ?? new Dictionary<string, RelationshipData>()).Add(rel.PublicRelationshipName, relData);
                }
            }

            return ro;
        }

        protected ResourceIdentifierObject GetRelatedResourceLinkage(HasOneAttribute attr, IIdentifiable entity)
        {
            var relatedEntity = (IIdentifiable)_resourceGraph.GetRelationshipValue(entity, attr);
            if (relatedEntity == null && IsRequiredToOneRelationship(attr, entity))
                throw new NotSupportedException("Cannot serialize a required to one relationship that is not populated but was included in the set of relationships to be serialized.");

            if (relatedEntity != null)
                return CreateResourceIdentifier(relatedEntity);

            return null;
        }

        protected List<ResourceIdentifierObject> GetRelatedResourceLinkage(HasManyAttribute attr, IIdentifiable entity)
        {
            var relatedEntities = (IEnumerable)_resourceGraph.GetRelationshipValue(entity, attr);
            var manyData = new List<ResourceIdentifierObject>();
            if (relatedEntities != null)
                foreach (IIdentifiable relatedEntity in relatedEntities)
                    manyData.Add(CreateResourceIdentifier(relatedEntity));

            return manyData;
        }

        /// <summary>
        /// Builds the <see cref="RelationshipData"/> entries of the "relationships"
        /// objects. The default behaviour is to just construct a resource linkage
        /// with the "data" field populated with "single" or "many" data.
        /// </summary>
        protected virtual RelationshipData GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            if (relationship is HasOneAttribute hasOne)
                return new RelationshipData { Data = GetRelatedResourceLinkage(hasOne, entity) };

            return new RelationshipData { Data = GetRelatedResourceLinkage((HasManyAttribute)relationship, entity) };
        }

        /// <summary>
        /// Creates a <see cref="ResourceIdentifierObject"/> from <paramref name="entity"/>.
        /// </summary>
        private ResourceIdentifierObject CreateResourceIdentifier(IIdentifiable entity)
        {
            var resourceName = _provider.GetContextEntity(entity.GetType()).EntityName;
            return new ResourceIdentifierObject
            {
                Type = resourceName,
                Id = entity.StringId
            };
        }

        /// <summary>
        /// Checks if the to-one relationship is required by checking if the foreign key is nullable.
        /// </summary>
        private bool IsRequiredToOneRelationship(HasOneAttribute attr, IIdentifiable entity)
        {
            var foreignKey = entity.GetType().GetProperty(attr.IdentifiablePropertyName);
            if (foreignKey != null && Nullable.GetUnderlyingType(foreignKey.PropertyType) == null)
                return true;

            return false;
        }
    }
}