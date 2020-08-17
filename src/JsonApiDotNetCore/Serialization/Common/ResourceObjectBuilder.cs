using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
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
        public ResourceObject Build(IIdentifiable resource, IEnumerable<AttrAttribute> attributes = null, IEnumerable<RelationshipAttribute> relationships = null)
        {
            var resourceContext = _provider.GetResourceContext(resource.GetType());

            // populating the top-level "type" and "id" members.
            var ro = new ResourceObject { Type = resourceContext.ResourceName, Id = resource.StringId == string.Empty ? null : resource.StringId };

            // populating the top-level "attribute" member of a resource object. never include "id" as an attribute
            if (attributes != null && (attributes = attributes.Where(attr => attr.Property.Name != _identifiablePropertyName)).Any())
                ProcessAttributes(resource, attributes, ro);

            // populating the top-level "relationship" member of a resource object.
            if (relationships != null)
                ProcessRelationships(resource, relationships, ro);

            return ro;
        }

        /// <summary>
        /// Builds the <see cref="RelationshipEntry"/> entries of the "relationships
        /// objects" The default behaviour is to just construct a resource linkage
        /// with the "data" field populated with "single" or "many" data.
        /// Depending on the requirements of the implementation (server or client serializer),
        /// this may be overridden.
        /// </summary>
        protected virtual RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            return new RelationshipEntry { Data = GetRelatedResourceLinkage(relationship, resource) };
        }

        /// <summary>
        /// Gets the value for the <see cref="ExposableData{T}.Data"/> property.
        /// </summary>
        protected object GetRelatedResourceLinkage(RelationshipAttribute relationship, IIdentifiable resource)
        {
            if (relationship is HasOneAttribute hasOne)
                return GetRelatedResourceLinkage(hasOne, resource);

            return GetRelatedResourceLinkage((HasManyAttribute)relationship, resource);
        }

        /// <summary>
        /// Builds a <see cref="ResourceIdentifierObject"/> for a HasOne relationship
        /// </summary>
        private ResourceIdentifierObject GetRelatedResourceLinkage(HasOneAttribute relationship, IIdentifiable resource)
        {
            var relatedResource = (IIdentifiable)relationship.GetValue(resource);
            if (relatedResource == null && IsRequiredToOneRelationship(relationship, resource))
                throw new NotSupportedException("Cannot serialize a required to one relationship that is not populated but was included in the set of relationships to be serialized.");

            if (relatedResource != null)
                return GetResourceIdentifier(relatedResource);

            return null;
        }

        /// <summary>
        /// Builds the <see cref="ResourceIdentifierObject"/>s for a HasMany relationship
        /// </summary>
        private List<ResourceIdentifierObject> GetRelatedResourceLinkage(HasManyAttribute relationship, IIdentifiable resource)
        {
            var relatedResources = (IEnumerable)relationship.GetValue(resource);
            var manyData = new List<ResourceIdentifierObject>();
            if (relatedResources != null)
                foreach (IIdentifiable relatedResource in relatedResources)
                    manyData.Add(GetResourceIdentifier(relatedResource));

            return manyData;
        }

        /// <summary>
        /// Creates a <see cref="ResourceIdentifierObject"/> from <paramref name="resource"/>.
        /// </summary>
        private ResourceIdentifierObject GetResourceIdentifier(IIdentifiable resource)
        {
            var resourceName = _provider.GetResourceContext(resource.GetType()).ResourceName;
            return new ResourceIdentifierObject
            {
                Type = resourceName,
                Id = resource.StringId
            };
        }

        /// <summary>
        /// Checks if the to-one relationship is required by checking if the foreign key is nullable.
        /// </summary>
        private bool IsRequiredToOneRelationship(HasOneAttribute attr, IIdentifiable resource)
        {
            var foreignKey = resource.GetType().GetProperty(attr.IdentifiablePropertyName);
            if (foreignKey != null && Nullable.GetUnderlyingType(foreignKey.PropertyType) == null)
                return true;

            return false;
        }

        /// <summary>
        /// Puts the relationships of the resource into the resource object.
        /// </summary>
        private void ProcessRelationships(IIdentifiable resource, IEnumerable<RelationshipAttribute> relationships, ResourceObject ro)
        {
            foreach (var rel in relationships)
            {
                var relData = GetRelationshipData(rel, resource);
                if (relData != null)
                    (ro.Relationships ??= new Dictionary<string, RelationshipEntry>()).Add(rel.PublicName, relData);
            }
        }

        /// <summary>
        /// Puts the attributes of the resource into the resource object.
        /// </summary>
        private void ProcessAttributes(IIdentifiable resource, IEnumerable<AttrAttribute> attributes, ResourceObject ro)
        {
            ro.Attributes = new Dictionary<string, object>();
            foreach (var attr in attributes)
            {
                object value = attr.GetValue(resource);

                if (_settings.SerializerNullValueHandling == NullValueHandling.Ignore && value == null)
                {
                    return;
                }

                if (_settings.SerializerDefaultValueHandling == DefaultValueHandling.Ignore && value == attr.Property.PropertyType.GetDefaultValue())
                {
                    return;
                }

                ro.Attributes.Add(attr.PublicName, value);
            }
        }
    }
}
