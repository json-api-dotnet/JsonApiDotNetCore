using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources.Internal;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <inheritdoc />
    [PublicAPI]
    public class ResourceObjectBuilder : IResourceObjectBuilder
    {
        private static readonly CollectionConverter CollectionConverter = new();
        private readonly IJsonApiOptions _options;

        protected IResourceGraph ResourceGraph { get; }

        public ResourceObjectBuilder(IResourceGraph resourceGraph, IJsonApiOptions options)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(options, nameof(options));

            ResourceGraph = resourceGraph;
            _options = options;
        }

        /// <inheritdoc />
        public virtual ResourceObject Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes = null,
            IReadOnlyCollection<RelationshipAttribute> relationships = null)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            ResourceContext resourceContext = ResourceGraph.GetResourceContext(resource.GetType());

            // populating the top-level "type" and "id" members.
            var resourceObject = new ResourceObject
            {
                Type = resourceContext.PublicName,
                Id = resource.StringId
            };

            // populating the top-level "attribute" member of a resource object. never include "id" as an attribute
            if (attributes != null)
            {
                AttrAttribute[] attributesWithoutId = attributes.Where(attr => attr.Property.Name != nameof(Identifiable.Id)).ToArray();

                if (attributesWithoutId.Any())
                {
                    ProcessAttributes(resource, attributesWithoutId, resourceObject);
                }
            }

            // populating the top-level "relationship" member of a resource object.
            if (relationships != null)
            {
                ProcessRelationships(resource, relationships, resourceObject);
            }

            return resourceObject;
        }

        /// <summary>
        /// Builds a <see cref="RelationshipObject" />. The default behavior is to just construct a resource linkage with the "data" field populated with
        /// "single" or "many" data.
        /// </summary>
        protected virtual RelationshipObject GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(resource, nameof(resource));

            return new RelationshipObject
            {
                Data = GetRelatedResourceLinkage(relationship, resource)
            };
        }

        /// <summary>
        /// Gets the value for the data property.
        /// </summary>
        protected SingleOrManyData<ResourceIdentifierObject> GetRelatedResourceLinkage(RelationshipAttribute relationship, IIdentifiable resource)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(resource, nameof(resource));

            return relationship is HasOneAttribute hasOne
                ? GetRelatedResourceLinkageForHasOne(hasOne, resource)
                : GetRelatedResourceLinkageForHasMany((HasManyAttribute)relationship, resource);
        }

        /// <summary>
        /// Builds a <see cref="ResourceIdentifierObject" /> for a HasOne relationship.
        /// </summary>
        private SingleOrManyData<ResourceIdentifierObject> GetRelatedResourceLinkageForHasOne(HasOneAttribute relationship, IIdentifiable resource)
        {
            var relatedResource = (IIdentifiable)relationship.GetValue(resource);
            ResourceIdentifierObject resourceIdentifierObject = relatedResource != null ? GetResourceIdentifier(relatedResource) : null;
            return new SingleOrManyData<ResourceIdentifierObject>(resourceIdentifierObject);
        }

        /// <summary>
        /// Builds the <see cref="ResourceIdentifierObject" />s for a HasMany relationship.
        /// </summary>
        private SingleOrManyData<ResourceIdentifierObject> GetRelatedResourceLinkageForHasMany(HasManyAttribute relationship, IIdentifiable resource)
        {
            object value = relationship.GetValue(resource);
            ICollection<IIdentifiable> relatedResources = CollectionConverter.ExtractResources(value);

            var manyData = new List<ResourceIdentifierObject>();

            if (relatedResources != null)
            {
                foreach (IIdentifiable relatedResource in relatedResources)
                {
                    manyData.Add(GetResourceIdentifier(relatedResource));
                }
            }

            return new SingleOrManyData<ResourceIdentifierObject>(manyData);
        }

        /// <summary>
        /// Creates a <see cref="ResourceIdentifierObject" /> from <paramref name="resource" />.
        /// </summary>
        private ResourceIdentifierObject GetResourceIdentifier(IIdentifiable resource)
        {
            string publicName = ResourceGraph.GetResourceContext(resource.GetType()).PublicName;

            return new ResourceIdentifierObject
            {
                Type = publicName,
                Id = resource.StringId
            };
        }

        /// <summary>
        /// Puts the relationships of the resource into the resource object.
        /// </summary>
        private void ProcessRelationships(IIdentifiable resource, IEnumerable<RelationshipAttribute> relationships, ResourceObject ro)
        {
            foreach (RelationshipAttribute rel in relationships)
            {
                RelationshipObject relData = GetRelationshipData(rel, resource);

                if (relData != null)
                {
                    (ro.Relationships ??= new Dictionary<string, RelationshipObject>()).Add(rel.PublicName, relData);
                }
            }
        }

        /// <summary>
        /// Puts the attributes of the resource into the resource object.
        /// </summary>
        private void ProcessAttributes(IIdentifiable resource, IEnumerable<AttrAttribute> attributes, ResourceObject ro)
        {
            ro.Attributes = new Dictionary<string, object>();

            foreach (AttrAttribute attr in attributes)
            {
                object value = attr.GetValue(resource);

                if (_options.SerializerOptions.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull && value == null)
                {
                    continue;
                }

                if (_options.SerializerOptions.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault &&
                    Equals(value, RuntimeTypeConverter.GetDefaultValue(attr.Property.PropertyType)))
                {
                    continue;
                }

                ro.Attributes.Add(attr.PublicName, value);
            }
        }
    }
}
