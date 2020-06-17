using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{

    /// <inheritdoc/> 
    public class ResourceObjectBuilder : IResourceObjectBuilder
    {
        protected readonly IResourceContextProvider _provider;
        private readonly ResourceObjectBuilderSettings _settings;
        private const string _identifiablePropertyName = nameof(Identifiable.Id);

        public ResourceObjectBuilder(IResourceContextProvider provider, ResourceObjectBuilderSettings settings)
        {
            _provider = provider;
            _settings = settings;
        }

        /// <inheritdoc/> 
        public ResourceObject Build(IIdentifiable entity, IEnumerable<AttrAttribute> attributes = null, IEnumerable<RelationshipAttribute> relationships = null)
        {
            var resourceContext = _provider.GetResourceContext(entity.GetType());

            // populating the top-level "type" and "id" members.
            var ro = new ResourceObject { Type = resourceContext.ResourceName, Id = entity.StringId == string.Empty ? null : entity.StringId };

            // populating the top-level "attribute" member of a resource object. never include "id" as an attribute
            if (attributes != null && (attributes = attributes.Where(attr => attr.PropertyInfo.Name != _identifiablePropertyName)).Any())
                ProcessAttributes(entity, attributes, ro);

            // populating the top-level "relationship" member of a resource object.
            if (relationships != null)
                ProcessRelationships(entity, relationships, ro);

            return ro;
        }

        /// <summary>
        /// Builds the <see cref="RelationshipEntry"/> entries of the "relationships
        /// objects" The default behaviour is to just construct a resource linkage
        /// with the "data" field populated with "single" or "many" data.
        /// Depending on the requirements of the implementation (server or client serializer),
        /// this may be overridden.
        /// </summary>
        protected virtual RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            return new RelationshipEntry { Data = GetRelatedResourceLinkage(relationship, entity) };
        }

        /// <summary>
        /// Gets the value for the <see cref="ExposableData{T}.Data"/> property.
        /// </summary>
        protected object GetRelatedResourceLinkage(RelationshipAttribute relationship, IIdentifiable entity)
        {
            if (relationship is HasOneAttribute hasOne)
                return GetRelatedResourceLinkage(hasOne, entity);

            return GetRelatedResourceLinkage((HasManyAttribute)relationship, entity);
        }

        /// <summary>
        /// Builds a <see cref="ResourceIdentifierObject"/> for a HasOne relationship
        /// </summary>
        private ResourceIdentifierObject GetRelatedResourceLinkage(HasOneAttribute relationship, IIdentifiable entity)
        {
            var relatedEntity = (IIdentifiable)relationship.GetValue(entity);
            if (relatedEntity == null && IsRequiredToOneRelationship(relationship, entity))
                throw new NotSupportedException("Cannot serialize a required to one relationship that is not populated but was included in the set of relationships to be serialized.");

            if (relatedEntity != null)
                return GetResourceIdentifier(relatedEntity);

            return null;
        }

        /// <summary>
        /// Builds the <see cref="ResourceIdentifierObject"/>s for a HasMany relationship
        /// </summary>
        private List<ResourceIdentifierObject> GetRelatedResourceLinkage(HasManyAttribute relationship, IIdentifiable entity)
        {
            var relatedEntities = (IEnumerable)relationship.GetValue(entity);
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
            var resourceName = _provider.GetResourceContext(entity.GetType()).ResourceName;
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

        /// <summary>
        /// Puts the relationships of the entity into the resource object.
        /// </summary>
        private void ProcessRelationships(IIdentifiable entity, IEnumerable<RelationshipAttribute> relationships, ResourceObject ro)
        {
            foreach (var rel in relationships)
            {
                var relData = GetRelationshipData(rel, entity);
                if (relData != null)
                    (ro.Relationships ??= new Dictionary<string, RelationshipEntry>()).Add(rel.PublicRelationshipName, relData);
            }
        }

        /// <summary>
        /// Puts the attributes of the entity into the resource object.
        /// </summary>
        private void ProcessAttributes(IIdentifiable entity, IEnumerable<AttrAttribute> attributes, ResourceObject ro)
        {
            ro.Attributes = new Dictionary<string, object>();
            foreach (var attr in attributes)
            {
                object value = attr.GetValue(entity);

                if (_settings.SerializerNullValueHandling == NullValueHandling.Ignore && value == null)
                {
                    continue;
                }

                if (_settings.SerializerDefaultValueHandling == DefaultValueHandling.Ignore && value == attr.PropertyInfo.PropertyType.GetDefaultValue())
                {
                    continue;
                }

                ro.Attributes.Add(attr.PublicAttributeName, value);
            }
        }
    }
}
