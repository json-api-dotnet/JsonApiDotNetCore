using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    /// <summary>
    /// Abstract base class for serialization. Converts entities in to <see cref="ResourceObject"/>s
    /// given a list of attributes and relationships.
    /// </summary>
    public abstract class ResourceObjectBuilder
    {
        protected readonly IResourceGraph _resourceGraph;
        protected readonly IContextEntityProvider _provider;
        private readonly SerializerSettings _settings;
        private const string _identifiablePropertyName = nameof(Identifiable.Id);

        protected ResourceObjectBuilder(IResourceGraph resourceGraph, IContextEntityProvider provider, SerializerSettings settings)
        {
            _resourceGraph = resourceGraph;
            _provider = provider;
            _settings = settings;
        }

        /// <summary>
        /// Converts <paramref name="entity"/> into a <see cref="ResourceObject"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attrs"/> and <paramref name="rels"/>
        /// </summary>
        /// <param name="entity">Entity to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        protected ResourceObject BuildResourceObject(IIdentifiable entity, IEnumerable<AttrAttribute> attributes, IEnumerable<RelationshipAttribute> relationships)
        {
            var resourceContext = _provider.GetContextEntity(entity.GetType());

            // populating the top-level "type" and "id" members.
            var ro = new ResourceObject { Type = resourceContext.EntityName, Id = entity.StringId.NullIfEmpty() };

            // populating the top-level "attribute" member of a resource object. never include "id" as an attribute
            attributes = attributes.Where(attr => attr.InternalAttributeName != _identifiablePropertyName);
            if (attributes.Any())
            {
                ro.Attributes = new Dictionary<string, object>();
                foreach (var attr in attributes)
                    AddAttribute(entity, ro, attr);
            }

            // populating the top-level "relationship" member of a resource object.
            foreach (var rel in relationships)
            {
                var relData = GetRelationshipData(rel, entity);
                if (relData != null)
                    (ro.Relationships = ro.Relationships ?? new Dictionary<string, RelationshipData>()).Add(rel.PublicRelationshipName, relData);
            }
            

            return ro;
        }


        private void AddAttribute(IIdentifiable entity, ResourceObject ro, AttrAttribute attr)
        {
            var value = attr.GetValue(entity);
            if ( !(value == default && _settings.OmitDefaultValuedAttributes) && !(value == null && _settings.OmitDefaultValuedAttributes))
                ro.Attributes.Add(attr.PublicAttributeName, value);
        }

        /// <summary>
        /// Builds the <see cref="RelationshipData"/> entries of the "relationships
        /// objects" The default behaviour is to just construct a resource linkage
        /// with the "data" field populated with "single" or "many" data.
        /// Depending on the requirements of the implementation (server or client serializer),
        /// this may be overridden.
        /// </summary>
        protected virtual RelationshipData GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            if (relationship is HasOneAttribute hasOne)
                return new RelationshipData { Data = GetRelatedResourceIdentifier(hasOne, entity) };

            return new RelationshipData { Data = GetRelatedResourceLinkage((HasManyAttribute)relationship, entity) };
        }

        /// <summary>
        /// Builds a <see cref="ResourceIdentifierObject"/> for a HasOne relationship
        /// </summary>
        protected ResourceIdentifierObject GetRelatedResourceIdentifier(HasOneAttribute attr, IIdentifiable entity)
        {
            var relatedEntity = (IIdentifiable)_resourceGraph.GetRelationshipValue(entity, attr);
            if (relatedEntity == null && IsRequiredToOneRelationship(attr, entity))
                throw new NotSupportedException("Cannot serialize a required to one relationship that is not populated but was included in the set of relationships to be serialized.");

            if (relatedEntity != null)
                return GetResourceIdentifier(relatedEntity);

            return null;
        }

        /// <summary>
        /// Builds the <see cref="ResourceIdentifierObject"/>s for a HasMany relationship
        /// </summary>
        protected List<ResourceIdentifierObject> GetRelatedResourceLinkage(HasManyAttribute attr, IIdentifiable entity)
        {
            var relatedEntities = (IEnumerable)_resourceGraph.GetRelationshipValue(entity, attr);
            var manyData = new List<ResourceIdentifierObject>();
            if (relatedEntities != null)
                foreach (IIdentifiable relatedEntity in relatedEntities)
                    manyData.Add(GetResourceIdentifier(relatedEntity));

            return manyData;
        }

        /// <summary>
        /// Creates a <see cref="ResourceIdentifierObject"/> from <paramref name="entity"/>.
        /// </summary>
        private ResourceIdentifierObject GetResourceIdentifier(IIdentifiable entity)
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