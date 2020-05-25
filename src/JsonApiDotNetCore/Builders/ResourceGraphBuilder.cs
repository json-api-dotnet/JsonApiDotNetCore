using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Fluent;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Reflection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Builders
{
    public class ResourceGraphBuilder: IResourceGraphBuilder
    {
        private readonly IJsonApiOptions _options;
        private readonly ILogger<ResourceGraphBuilder> _logger;
        private readonly List<ResourceContext> _resources = new List<ResourceContext>();
        private readonly IResourceMappingService _resourceMappingService;
        
        public ResourceGraphBuilder(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceMappingService resourceMappingService = null)
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<ResourceGraphBuilder>();
            _resourceMappingService = resourceMappingService;
        }

        /// <inheritdoc />
        public IResourceGraph Build()
        {
            _resources.ForEach(SetResourceLinksOptions);
            return new ResourceGraph(_resources);
        }

        private void SetResourceLinksOptions(ResourceContext resourceContext)
        {
            LinksAttribute attribute = GetResourceLinksOptionsWithPrecedenceToMappingOverAnnotation(resourceContext);

            if (attribute != null)
            {
                resourceContext.RelationshipLinks = attribute.RelationshipLinks;
                resourceContext.ResourceLinks = attribute.ResourceLinks;
                resourceContext.TopLevelLinks = attribute.TopLevelLinks;
            }
        }

        private LinksAttribute GetResourceLinksOptionsWithPrecedenceToMappingOverAnnotation(ResourceContext resourceContext)
        {
            LinksAttribute annotation = GetResourceLinksOptionsFromAnnotations(resourceContext);

            LinksAttribute mapping = GetResourceLinksOptionsFromMappings(resourceContext);

            LinksAttribute attribute = mapping != null ? mapping : annotation;

            return attribute;
        }

        private LinksAttribute GetResourceLinksOptionsFromAnnotations(ResourceContext resourceContext)
        {
            LinksAttribute attribute = (LinksAttribute)resourceContext.ResourceType.GetCustomAttribute(typeof(LinksAttribute));

            return attribute;
        }

        private LinksAttribute GetResourceLinksOptionsFromMappings(ResourceContext resourceContext)
        {
            LinksAttribute attribute = null;

            IResourceMapping mapping = null;

            if (_resourceMappingService != null && _resourceMappingService.TryGetResourceMapping(resourceContext.ResourceType, out mapping))
            {
                attribute = mapping.Links;
            }

            return attribute;
        }

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource<TResource>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<int>
            => AddResource<TResource, int>(pluralizedTypeName);

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource<TResource, TId>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<TId>
            => AddResource(typeof(TResource), typeof(TId), pluralizedTypeName);

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource(Type resourceType, Type idType = null, string pluralizedTypeName = null)
        {
            if (_resources.All(e => e.ResourceType != resourceType))
            {
                if (resourceType.IsOrImplementsInterface(typeof(IIdentifiable)))
                {
                    pluralizedTypeName ??= FormatResourceName(resourceType);
                    idType ??= TypeLocator.GetIdType(resourceType);
                    var resourceContext = CreateResourceContext(pluralizedTypeName, resourceType, idType);
                    _resources.Add(resourceContext);
                }
                else
                {
                    _logger.LogWarning($"Entity '{resourceType}' does not implement '{nameof(IIdentifiable)}'.");
                }
            }

            return this;
        }

        private ResourceContext CreateResourceContext(string pluralizedTypeName, Type entityType, Type idType) => new ResourceContext
        {
            ResourceName = pluralizedTypeName,
            ResourceType = entityType,
            IdentityType = idType,
            Attributes = GetAttributes(entityType),
            Relationships = GetRelationships(entityType),
            EagerLoads = GetEagerLoads(entityType),
            ResourceDefinitionType = GetResourceDefinitionType(entityType)
        };

        protected virtual List<AttrAttribute> GetAttributes(Type entityType)
        {
            List<AttrAttribute> attributes = new List<AttrAttribute>();

            List<AttrAttribute> annotations = GetAttributesFromAnnotations(entityType);

            List<AttrAttribute> mappings = GetAttributesFromMapping(entityType);

            attributes = CombineWithPrecedenceToMappingOverAnnotation(mappings, annotations, AttrAttributeComparer.Instance);

            return attributes;
        }

        protected virtual List<AttrAttribute> GetAttributesFromAnnotations(Type entityType)
        {
            var attributes = new List<AttrAttribute>();

            foreach (var property in entityType.GetProperties())
            {
                var attribute = (AttrAttribute)property.GetCustomAttribute(typeof(AttrAttribute));

                // TODO: investigate why this is added in the exposed attributes list
                // because it is not really defined attribute considered from the json:api
                // spec point of view.
                if (property.Name == nameof(Identifiable.Id) && attribute == null)
                {
                    var idAttr = new AttrAttribute
                    {
                        PublicAttributeName = FormatPropertyName(property),
                        PropertyInfo = property,
                        Capabilities = _options.DefaultAttrCapabilities
                    };
                    attributes.Add(idAttr);
                    continue;
                }

                if (attribute == null)
                    continue;

                attribute.PublicAttributeName ??= FormatPropertyName(property);
                attribute.PropertyInfo = property;

                if (!attribute.HasExplicitCapabilities)
                {
                    attribute.Capabilities = _options.DefaultAttrCapabilities;
                }

                attributes.Add(attribute);
            }
            return attributes;
        }

        protected virtual List<AttrAttribute> GetAttributesFromMapping(Type entityType)
        {
            var attributes = new List<AttrAttribute>();

            IResourceMapping mapping;

            if (_resourceMappingService != null && _resourceMappingService.TryGetResourceMapping(entityType, out mapping))                
            {
                foreach (var attribute in mapping.Attributes)
                {
                    attribute.PublicAttributeName ??= FormatPropertyName(attribute.PropertyInfo);
                    attribute.PropertyInfo = attribute.PropertyInfo;

                    if (!attribute.HasExplicitCapabilities)
                    {
                        attribute.Capabilities = _options.DefaultAttrCapabilities;
                    }

                    attributes.Add(attribute);
                }
            }

            return attributes;
        }

        protected virtual List<RelationshipAttribute> GetRelationships(Type entityType)
        {
            List<RelationshipAttribute> attributes = new List<RelationshipAttribute>();

            List<RelationshipAttribute> annotations = GetRelationshipsFromAnnotations(entityType);

            List<RelationshipAttribute> mappings = GetRelationshipsFromMapping(entityType);

            attributes = CombineWithPrecedenceToMappingOverAnnotation(mappings, annotations, RelationshipAttributeComparer.Instance);

            return attributes;
        }

        protected List<RelationshipAttribute> GetRelationshipsFromAnnotations(Type entityType)
        {
            var attributes = new List<RelationshipAttribute>();
            var properties = entityType.GetProperties();

            foreach (var prop in properties)
            {
                var attribute = (RelationshipAttribute)prop.GetCustomAttribute(typeof(RelationshipAttribute));
                if (attribute == null) continue;

                attribute.PropertyInfo = prop;
                attribute.PublicRelationshipName ??= FormatPropertyName(prop);
                attribute.RightType = GetRelationshipType(attribute, prop);
                attribute.LeftType = entityType;
                attributes.Add(attribute);

                if (attribute is HasManyThroughAttribute hasManyThroughAttribute)
                {
                    var throughProperty = properties.SingleOrDefault(p => p.Name == hasManyThroughAttribute.ThroughPropertyName);
                    if (throughProperty == null)
                        throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{entityType}.{attribute.PropertyInfo.Name}': Resource does not contain a property named '{hasManyThroughAttribute.ThroughPropertyName}'.");

                    var throughType = TryGetThroughType(throughProperty);
                    if (throughType == null)
                        throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{entityType}.{attribute.PropertyInfo.Name}': Referenced property '{throughProperty.Name}' does not implement 'ICollection<T>'.");

                    // ICollection<ArticleTag>
                    hasManyThroughAttribute.ThroughProperty = throughProperty;

                    // ArticleTag
                    hasManyThroughAttribute.ThroughType = throughType;

                    var throughProperties = throughType.GetProperties();

                    // ArticleTag.Article
                    hasManyThroughAttribute.LeftProperty = throughProperties.SingleOrDefault(x => x.PropertyType == entityType)
                        ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {entityType}");

                    // ArticleTag.ArticleId
                    var leftIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.LeftProperty.Name);
                    hasManyThroughAttribute.LeftIdProperty = throughProperties.SingleOrDefault(x => x.Name == leftIdPropertyName)
                        ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {entityType} with name {leftIdPropertyName}");

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

        protected List<RelationshipAttribute> GetRelationshipsFromMapping(Type entityType)
        {
            var attributes = new List<RelationshipAttribute>();

            IResourceMapping mapping;

            if (_resourceMappingService != null && _resourceMappingService.TryGetResourceMapping(entityType, out mapping))                
            {
                foreach (var relationship in mapping.Relationships)
                {
                    var attribute = relationship;
                    if (attribute == null) continue;

                    attribute.PropertyInfo = relationship.PropertyInfo;
                    attribute.PublicRelationshipName ??= FormatPropertyName(relationship.PropertyInfo);
                    attribute.RightType = GetRelationshipType(attribute, relationship.PropertyInfo);
                    attribute.LeftType = entityType;
                    attributes.Add(attribute);

                    if (attribute is HasManyThroughAttribute hasManyThroughAttribute)
                    {
                        var throughProperty = hasManyThroughAttribute.ThroughProperty;

                        if (throughProperty == null)
                            throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{entityType}.{attribute.PropertyInfo.Name}': Resource does not contain a property named '{hasManyThroughAttribute.ThroughPropertyName}'.");

                        var throughType = TryGetThroughType(throughProperty);
                        if (throughType == null)
                            throw new JsonApiSetupException($"Invalid {nameof(HasManyThroughAttribute)} on '{entityType}.{attribute.PropertyInfo.Name}': Referenced property '{throughProperty.Name}' does not implement 'ICollection<T>'.");

                        // ICollection<ArticleTag>
                        hasManyThroughAttribute.ThroughProperty = throughProperty;

                        // ArticleTag
                        hasManyThroughAttribute.ThroughType = throughType;

                        var throughProperties = throughType.GetProperties();

                        // ArticleTag.Article
                        hasManyThroughAttribute.LeftProperty = throughProperties.SingleOrDefault(x => x.PropertyType == entityType)
                            ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {entityType}");

                        // ArticleTag.ArticleId
                        var leftIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.LeftProperty.Name);
                        hasManyThroughAttribute.LeftIdProperty = throughProperties.SingleOrDefault(x => x.Name == leftIdPropertyName)
                            ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {entityType} with name {leftIdPropertyName}");

                        // ArticleTag.Tag
                        hasManyThroughAttribute.RightProperty = throughProperties.SingleOrDefault(x => x.PropertyType == hasManyThroughAttribute.RightType)
                            ?? throw new JsonApiSetupException($"{throughType} does not contain a navigation property to type {hasManyThroughAttribute.RightType}");

                        // ArticleTag.TagId
                        var rightIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.RightProperty.Name);
                        hasManyThroughAttribute.RightIdProperty = throughProperties.SingleOrDefault(x => x.Name == rightIdPropertyName)
                            ?? throw new JsonApiSetupException($"{throughType} does not contain a relationship id property to type {hasManyThroughAttribute.RightType} with name {rightIdPropertyName}");
                    }
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

        private List<EagerLoadAttribute> GetEagerLoads(Type entityType, int recursionDepth = 0)
        {
            if (recursionDepth >= 500)
            {
                throw new InvalidOperationException("Infinite recursion detected in eager-load chain.");
            }

            List<EagerLoadAttribute> attributes = new List<EagerLoadAttribute>();

            List<EagerLoadAttribute> annotations = GetEagerLoadsFromAnnotations(entityType, recursionDepth);

            List<EagerLoadAttribute> mappings = GetEagerLoadsFromMapping(entityType, recursionDepth);

            attributes = CombineWithPrecedenceToMappingOverAnnotation(mappings, annotations, EagerLoadAttributeComparer.Instance);

            return attributes;
        }

        private List<EagerLoadAttribute> GetEagerLoadsFromAnnotations(Type entityType, int recursionDepth = 0)
        {
            var attributes = new List<EagerLoadAttribute>();
            var properties = entityType.GetProperties();

            foreach (var property in properties)
            {
                var attribute = (EagerLoadAttribute)property.GetCustomAttribute(typeof(EagerLoadAttribute));
                if (attribute == null) continue;

                Type innerType = TypeOrElementType(property.PropertyType);
                attribute.Children = GetEagerLoads(innerType, recursionDepth + 1);
                attribute.Property = property;

                attributes.Add(attribute);
            }

            return attributes;
        }

        private List<EagerLoadAttribute> GetEagerLoadsFromMapping(Type entityType, int recursionDepth = 0)
        {
            var attributes = new List<EagerLoadAttribute>();

            IResourceMapping mapping;

            if (_resourceMappingService != null && _resourceMappingService.TryGetResourceMapping(entityType, out mapping))
            {
                foreach (var eagerLoad in mapping.EagerLoads)
                {
                    Type innerType = TypeOrElementType(eagerLoad.Property.PropertyType);
                    eagerLoad.Children = GetEagerLoads(innerType, recursionDepth + 1);
                    eagerLoad.Property = eagerLoad.Property;

                    attributes.Add(eagerLoad);
                }
            }

            return attributes;
        }

        private static Type TypeOrElementType(Type type)
        {
            var interfaces = type.GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();

            return interfaces.Length == 1 ? interfaces.Single().GenericTypeArguments[0] : type;
        }

        private Type GetResourceDefinitionType(Type entityType) => typeof(ResourceDefinition<>).MakeGenericType(entityType);

        private string FormatResourceName(Type resourceType)
        {
            var formatter = new ResourceNameFormatter(_options, _resourceMappingService);
            return formatter.FormatResourceName(resourceType);
        }

        private string FormatPropertyName(PropertyInfo resourceProperty)
        {
            return _options.SerializerContractResolver.NamingStrategy.GetPropertyName(resourceProperty.Name, false);
        }

        private static List<T> CombineWithPrecedenceToMappingOverAnnotation<T>(List<T> mappings, List<T> annotations, IEqualityComparer<T> comparer)
        {
            return mappings.Union(annotations, comparer)
                           .ToList();
        }
    }
}
