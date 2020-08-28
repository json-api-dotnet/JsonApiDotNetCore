using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Builders
{
    public class ResourceGraphBuilder
    {
        private readonly IJsonApiOptions _options;
        private readonly ILogger<ResourceGraphBuilder> _logger;
        private readonly List<ResourceContext> _resources = new List<ResourceContext>();

        public ResourceGraphBuilder(IJsonApiOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _logger = loggerFactory?.CreateLogger<ResourceGraphBuilder>();
        }

        /// <inheritdoc />
        public IResourceGraph Build()
        {
            _resources.ForEach(SetResourceLinksOptions);
            return new ResourceGraph(_resources);
        }

        private void SetResourceLinksOptions(ResourceContext resourceContext)
        {
            var attribute = (LinksAttribute)resourceContext.ResourceType.GetCustomAttribute(typeof(LinksAttribute));
            if (attribute != null)
            {
                resourceContext.RelationshipLinks = attribute.RelationshipLinks;
                resourceContext.ResourceLinks = attribute.ResourceLinks;
                resourceContext.TopLevelLinks = attribute.TopLevelLinks;
            }
        }

        /// <inheritdoc />
        public ResourceGraphBuilder AddResource<TResource>(string publicResourceName = null) where TResource : class, IIdentifiable<int>
            => AddResource<TResource, int>(publicResourceName);

        /// <inheritdoc />
        public ResourceGraphBuilder AddResource<TResource, TId>(string publicResourceName = null) where TResource : class, IIdentifiable<TId>
            => AddResource(typeof(TResource), typeof(TId), publicResourceName);

        /// <inheritdoc />
        public ResourceGraphBuilder AddResource(Type resourceType, Type idType = null, string publicResourceName = null)
        {
            if (_resources.All(e => e.ResourceType != resourceType))
            {
                if (resourceType.IsOrImplementsInterface(typeof(IIdentifiable)))
                {
                    publicResourceName ??= FormatResourceName(resourceType);
                    idType ??= TypeLocator.GetIdType(resourceType);
                    var resourceContext = CreateResourceContext(publicResourceName, resourceType, idType);
                    _resources.Add(resourceContext);
                }
                else
                {
                    _logger?.LogWarning($"Entity '{resourceType}' does not implement '{nameof(IIdentifiable)}'.");
                }
            }

            return this;
        }

        private ResourceContext CreateResourceContext(string publicResourceName, Type resourceType, Type idType) => new ResourceContext
        {
            ResourceName = publicResourceName,
            ResourceType = resourceType,
            IdentityType = idType,
            Attributes = GetAttributes(resourceType),
            Relationships = GetRelationships(resourceType),
            EagerLoads = GetEagerLoads(resourceType),
            ResourceDefinitionType = GetResourceDefinitionType(resourceType)
        };

        protected virtual List<AttrAttribute> GetAttributes(Type resourceType)
        {
            var attributes = new List<AttrAttribute>();

            foreach (var property in resourceType.GetProperties())
            {
                var attribute = (AttrAttribute)property.GetCustomAttribute(typeof(AttrAttribute));

                // Although strictly not correct, 'id' is added to the list of attributes for convenience.
                // For example, it enables to filter on id, without the need to special-case existing logic.
                // And when using sparse fields, it silently adds 'id' to the set of attributes to retrieve.
                if (property.Name == nameof(Identifiable.Id) && attribute == null)
                {
                    var idAttr = new AttrAttribute
                    {
                        PublicName = FormatPropertyName(property),
                        Property = property,
                        Capabilities = _options.DefaultAttrCapabilities
                    };
                    attributes.Add(idAttr);
                    continue;
                }

                if (attribute == null)
                    continue;

                attribute.PublicName ??= FormatPropertyName(property);
                attribute.Property = property;

                if (!attribute.HasExplicitCapabilities)
                {
                    attribute.Capabilities = _options.DefaultAttrCapabilities;
                }

                attributes.Add(attribute);
            }
            return attributes;
        }

        protected virtual List<RelationshipAttribute> GetRelationships(Type resourceType)
        {
            var attributes = new List<RelationshipAttribute>();
            var properties = resourceType.GetProperties();
            foreach (var prop in properties)
            {
                var attribute = (RelationshipAttribute)prop.GetCustomAttribute(typeof(RelationshipAttribute));
                if (attribute == null) continue;

                attribute.Property = prop;
                attribute.PublicName ??= FormatPropertyName(prop);
                attribute.RightType = GetRelationshipType(attribute, prop);
                attribute.LeftType = resourceType;
                attributes.Add(attribute);

                if (attribute is HasManyThroughAttribute hasManyThroughAttribute)
                {
                    var throughProperty = properties.SingleOrDefault(p => p.Name == hasManyThroughAttribute.ThroughPropertyName);
                    if (throughProperty == null)
                        throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{resourceType}.{attribute.Property.Name}': Resource does not contain a property named '{hasManyThroughAttribute.ThroughPropertyName}'.");

                    var throughType = TryGetThroughType(throughProperty);
                    if (throughType == null)
                        throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{resourceType}.{attribute.Property.Name}': Referenced property '{throughProperty.Name}' does not implement 'ICollection<T>'.");

                    // ICollection<ArticleTag>
                    hasManyThroughAttribute.ThroughProperty = throughProperty;

                    // ArticleTag
                    hasManyThroughAttribute.ThroughType = throughType;

                    var throughProperties = throughType.GetProperties();

                    // ArticleTag.Article
                    hasManyThroughAttribute.LeftProperty = throughProperties.SingleOrDefault(x => x.PropertyType == resourceType)
                        ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {resourceType}");

                    // ArticleTag.ArticleId
                    var leftIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.LeftProperty.Name);
                    hasManyThroughAttribute.LeftIdProperty = throughProperties.SingleOrDefault(x => x.Name == leftIdPropertyName)
                        ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {resourceType} with name {leftIdPropertyName}");

                    // ArticleTag.Tag
                    hasManyThroughAttribute.RightProperty = throughProperties.SingleOrDefault(x => x.PropertyType == hasManyThroughAttribute.RightType)
                        ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {hasManyThroughAttribute.RightType}");

                    // ArticleTag.TagId
                    var rightIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.RightProperty.Name);
                    hasManyThroughAttribute.RightIdProperty = throughProperties.SingleOrDefault(x => x.Name == rightIdPropertyName)
                        ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {hasManyThroughAttribute.RightType} with name {rightIdPropertyName}");
                }
            }

            return attributes;
        }

        private static Type TryGetThroughType(PropertyInfo throughProperty)
        {
            if (throughProperty.PropertyType.IsGenericType)
            {
                var typeArguments = throughProperty.PropertyType.GetGenericArguments();
                if (typeArguments.Length == 1)
                {
                    var constructedThroughType = typeof(ICollection<>).MakeGenericType(typeArguments[0]);
                    if (throughProperty.PropertyType.IsOrImplementsInterface(constructedThroughType))
                    {
                        return typeArguments[0];
                    }
                }
            }

            return null;
        }

        protected virtual Type GetRelationshipType(RelationshipAttribute relation, PropertyInfo prop) =>
            relation is HasOneAttribute ? prop.PropertyType : prop.PropertyType.GetGenericArguments()[0];

        private List<EagerLoadAttribute> GetEagerLoads(Type resourceType, int recursionDepth = 0)
        {
            if (recursionDepth >= 500)
            {
                throw new InvalidOperationException("Infinite recursion detected in eager-load chain.");
            }

            var attributes = new List<EagerLoadAttribute>();
            var properties = resourceType.GetProperties();

            foreach (var property in properties)
            {
                var attribute = (EagerLoadAttribute) property.GetCustomAttribute(typeof(EagerLoadAttribute));
                if (attribute == null) continue;

                Type innerType = TypeOrElementType(property.PropertyType);
                attribute.Children = GetEagerLoads(innerType, recursionDepth + 1);
                attribute.Property = property;

                attributes.Add(attribute);
            }

            return attributes;
        }

        private static Type TypeOrElementType(Type type)
        {
            var interfaces = type.GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();

            return interfaces.Length == 1 ? interfaces.Single().GenericTypeArguments[0] : type;
        }

        private Type GetResourceDefinitionType(Type resourceType) => typeof(ResourceDefinition<>).MakeGenericType(resourceType);

        private string FormatResourceName(Type resourceType)
        {
            var formatter = new ResourceNameFormatter(_options);
            return formatter.FormatResourceName(resourceType);
        }

        private string FormatPropertyName(PropertyInfo resourceProperty)
        {
            return _options.SerializerContractResolver.NamingStrategy.GetPropertyName(resourceProperty.Name, false);
        }
    }
}
