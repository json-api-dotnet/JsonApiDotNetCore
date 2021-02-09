using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <inheritdoc /> 
    public class ResourceObjectBuilder : IResourceObjectBuilder
    {
        protected IResourceContextProvider ResourceContextProvider { get; }
        private readonly ResourceObjectBuilderSettings _settings;

        public ResourceObjectBuilder(IResourceContextProvider resourceContextProvider, ResourceObjectBuilderSettings settings)
        {
            ResourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc /> 
        public virtual ResourceObject Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes = null, IReadOnlyCollection<RelationshipAttribute> relationships = null)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var resourceContext = ResourceContextProvider.GetResourceContext(resource.GetType());

            // populating the top-level "type" and "id" members.
            var resourceObject = new ResourceObject { Type = resourceContext.PublicName, Id = resource.StringId };

            // populating the top-level "attribute" member of a resource object. never include "id" as an attribute
            if (attributes != null && (attributes = attributes.Where(attr => attr.Property.Name != nameof(Identifiable.Id)).ToArray()).Any())
                ProcessAttributes(resource, attributes, resourceObject);

            // populating the top-level "relationship" member of a resource object.
            if (relationships != null)
                ProcessRelationships(resource, relationships, resourceObject);

            return resourceObject;
        }

        /// <summary>
        /// Builds the <see cref="RelationshipEntry"/> entries of the "relationships
        /// objects". The default behavior is to just construct a resource linkage
        /// with the "data" field populated with "single" or "many" data.
        /// Depending on the requirements of the implementation (server or client serializer),
        /// this may be overridden.
        /// </summary>
        protected virtual RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            return new RelationshipEntry { Data = GetRelatedResourceLinkage(relationship, resource) };
        }

        /// <summary>
        /// Gets the value for the <see cref="ExposableData{T}.Data"/> property.
        /// </summary>
        protected object GetRelatedResourceLinkage(RelationshipAttribute relationship, IIdentifiable resource)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            return relationship is HasOneAttribute hasOne
                ? (object) GetRelatedResourceLinkageForHasOne(hasOne, resource)
                : GetRelatedResourceLinkageForHasMany((HasManyAttribute) relationship, resource);
        }

        /// <summary>
        /// Builds a <see cref="ResourceIdentifierObject"/> for a HasOne relationship.
        /// </summary>
        private ResourceIdentifierObject GetRelatedResourceLinkageForHasOne(HasOneAttribute relationship, IIdentifiable resource)
        {
            var relatedResource = (IIdentifiable)relationship.GetValue(resource);

            if (relatedResource != null)
            {
                return GetResourceIdentifier(relatedResource);
            }

            return null;
        }

        /// <summary>
        /// Builds the <see cref="ResourceIdentifierObject"/>s for a HasMany relationship.
        /// </summary>
        private List<ResourceIdentifierObject> GetRelatedResourceLinkageForHasMany(HasManyAttribute relationship, IIdentifiable resource)
        {
            var value = relationship.GetValue(resource);
            var relatedResources = TypeHelper.ExtractResources(value);
            
            var manyData = new List<ResourceIdentifierObject>();
            if (relatedResources != null)
            {
                foreach (var relatedResource in relatedResources)
                {
                    manyData.Add(GetResourceIdentifier(relatedResource));
                }
            }

            return manyData;
        }

        /// <summary>
        /// Creates a <see cref="ResourceIdentifierObject"/> from <paramref name="resource"/>.
        /// </summary>
        private ResourceIdentifierObject GetResourceIdentifier(IIdentifiable resource)
        {
            var resourceName = ResourceContextProvider.GetResourceContext(resource.GetType()).PublicName;
            return new ResourceIdentifierObject
            {
                Type = resourceName,
                Id = resource.StringId
            };
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

                if (_settings.SerializerDefaultValueHandling == DefaultValueHandling.Ignore && Equals(value, TypeHelper.GetDefaultValue(attr.Property.PropertyType)))
                {
                    return;
                }

                ro.Attributes.Add(attr.PublicName, value);
            }
        }
    }
}
