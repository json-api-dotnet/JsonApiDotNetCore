using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Builders
{
    public class ResourceGraphBuilder : IResourceGraphBuilder
    {
        private readonly IJsonApiOptions _options;
        private readonly List<ResourceContext> _resources = new List<ResourceContext>();
        private readonly List<ValidationResult> _validationResults = new List<ValidationResult>();

        public ResourceGraphBuilder(IJsonApiOptions options)
        {
            _options = options;
        }

        /// <inheritdoc />
        public IResourceGraph Build()
        {
            _resources.ForEach(SetResourceLinksOptions);
            var resourceGraph = new ResourceGraph(_resources, _validationResults);
            return resourceGraph;
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
        public IResourceGraphBuilder AddResource<TResource>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<int>
            => AddResource<TResource, int>(pluralizedTypeName);

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource<TResource, TId>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<TId>
            => AddResource(typeof(TResource), typeof(TId), pluralizedTypeName);

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource(Type resourceType, Type idType = null, string pluralizedTypeName = null)
        {
            AssertEntityIsNotAlreadyDefined(resourceType);
            if (resourceType.Implements<IIdentifiable>())
            {
                pluralizedTypeName ??= FormatResourceName(resourceType);
                idType ??= TypeLocator.GetIdType(resourceType);
                _resources.Add(GetEntity(pluralizedTypeName, resourceType, idType));
            }
            else
            {
                _validationResults.Add(new ValidationResult(LogLevel.Warning, $"{resourceType} does not implement 'IIdentifiable<>'. "));
            }

            return this;
        }

        private ResourceContext GetEntity(string pluralizedTypeName, Type entityType, Type idType) => new ResourceContext
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

        protected virtual List<RelationshipAttribute> GetRelationships(Type entityType)
        {
            var attributes = new List<RelationshipAttribute>();
            var properties = entityType.GetProperties();
            foreach (var prop in properties)
            {
                var attribute = (RelationshipAttribute)prop.GetCustomAttribute(typeof(RelationshipAttribute));
                if (attribute == null) continue;

                attribute.PublicRelationshipName ??= FormatPropertyName(prop);
                attribute.InternalRelationshipName = prop.Name;
                attribute.RightType = GetRelationshipType(attribute, prop);
                attribute.LeftType = entityType;
                attributes.Add(attribute);

                if (attribute is HasManyThroughAttribute hasManyThroughAttribute)
                {
                    var throughProperty = properties.SingleOrDefault(p => p.Name == hasManyThroughAttribute.InternalThroughName);
                    if (throughProperty == null)
                        throw new JsonApiSetupException($"Invalid '{nameof(HasManyThroughAttribute)}' on type '{entityType}'. Type does not contain a property named '{hasManyThroughAttribute.InternalThroughName}'.");

                    if (throughProperty.PropertyType.Implements<IList>() == false)
                        throw new JsonApiSetupException($"Invalid '{nameof(HasManyThroughAttribute)}' on type '{entityType}.{throughProperty.Name}'. Property type does not implement IList.");

                    // assumption: the property should be a generic collection, e.g. List<ArticleTag>
                    if (throughProperty.PropertyType.IsGenericType == false)
                        throw new JsonApiSetupException($"Invalid '{nameof(HasManyThroughAttribute)}' on type '{entityType}'. Expected through entity to be a generic type, such as List<{prop.PropertyType}>.");

                    // Article → List<ArticleTag>
                    hasManyThroughAttribute.ThroughProperty = throughProperty;

                    // ArticleTag
                    hasManyThroughAttribute.ThroughType = throughProperty.PropertyType.GetGenericArguments()[0];

                    var throughProperties = hasManyThroughAttribute.ThroughType.GetProperties();

                    // ArticleTag.Article
                    hasManyThroughAttribute.LeftProperty = throughProperties.SingleOrDefault(x => x.PropertyType == entityType)
                        ?? throw new JsonApiSetupException($"{hasManyThroughAttribute.ThroughType} does not contain a navigation property to type {entityType}");

                    // ArticleTag.ArticleId
                    var leftIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.LeftProperty.Name);
                    hasManyThroughAttribute.LeftIdProperty = throughProperties.SingleOrDefault(x => x.Name == leftIdPropertyName)
                        ?? throw new JsonApiSetupException($"{hasManyThroughAttribute.ThroughType} does not contain a relationship id property to type {entityType} with name {leftIdPropertyName}");

                    // Article → ArticleTag.Tag
                    hasManyThroughAttribute.RightProperty = throughProperties.SingleOrDefault(x => x.PropertyType == hasManyThroughAttribute.RightType)
                        ?? throw new JsonApiSetupException($"{hasManyThroughAttribute.ThroughType} does not contain a navigation property to type {hasManyThroughAttribute.RightType}");

                    // ArticleTag.TagId
                    var rightIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.RightProperty.Name);
                    hasManyThroughAttribute.RightIdProperty = throughProperties.SingleOrDefault(x => x.Name == rightIdPropertyName)
                        ?? throw new JsonApiSetupException($"{hasManyThroughAttribute.ThroughType} does not contain a relationship id property to type {hasManyThroughAttribute.RightType} with name {rightIdPropertyName}");
                }
            }

            return attributes;
        }

        protected virtual Type GetRelationshipType(RelationshipAttribute relation, PropertyInfo prop) =>
            relation.IsHasMany ? prop.PropertyType.GetGenericArguments()[0] : prop.PropertyType;

        private List<EagerLoadAttribute> GetEagerLoads(Type entityType, int recursionDepth = 0)
        {
            if (recursionDepth >= 500)
            {
                throw new InvalidOperationException("Infinite recursion detected in eager-load chain.");
            }

            var attributes = new List<EagerLoadAttribute>();
            var properties = entityType.GetProperties();

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

        private Type GetResourceDefinitionType(Type entityType) => typeof(ResourceDefinition<>).MakeGenericType(entityType);

        private void AssertEntityIsNotAlreadyDefined(Type entityType)
        {
            if (_resources.Any(e => e.ResourceType == entityType))
                throw new InvalidOperationException($"Cannot add entity type {entityType} to context resourceGraph, there is already an entity of that type configured.");
        }

        private string FormatResourceName(Type resourceType)
        {
            var formatter = new ResourceNameFormatter(_options);
            return formatter.FormatResourceName(resourceType);
        }

        private string FormatPropertyName(PropertyInfo resourceProperty)
        {
            var contractResolver = (DefaultContractResolver)_options.SerializerSettings.ContractResolver;
            return contractResolver.NamingStrategy.GetPropertyName(resourceProperty.Name, false);
        }
    }
}
