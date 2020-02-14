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

namespace JsonApiDotNetCore.Builders
{
    public class ResourceGraphBuilder : IResourceGraphBuilder
    {
        private List<ResourceContext> _resources { get; set; } = new List<ResourceContext>();
        private List<ValidationResult> _validationResults { get; set; } = new List<ValidationResult>();
        private IResourceNameFormatter _formatter { get; set; } = new CamelCaseFormatter();

        public ResourceGraphBuilder() { }

        public ResourceGraphBuilder(IResourceNameFormatter formatter)
        {
            _formatter = formatter;
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
                pluralizedTypeName ??= _formatter.FormatResourceName(resourceType);
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
            ResourceDefinitionType = GetResourceDefinitionType(entityType)
        };


        protected virtual List<AttrAttribute> GetAttributes(Type entityType)
        {
            var attributes = new List<AttrAttribute>();

            var properties = entityType.GetProperties();

            foreach (var prop in properties)
            {
                // todo: investigate why this is added in the exposed attributes list
                // because it is not really defined attribute considered from the json:api
                // spec point of view.
                if (prop.Name == nameof(Identifiable.Id))
                {
                    var idAttr = new AttrAttribute()
                    {
                        PublicAttributeName = _formatter.FormatPropertyName(prop),
                        PropertyInfo = prop
                    };
                    attributes.Add(idAttr);
                    continue;
                }

                var attribute = (AttrAttribute)prop.GetCustomAttribute(typeof(AttrAttribute));
                if (attribute == null)
                    continue;

                attribute.PublicAttributeName ??= _formatter.FormatPropertyName(prop);
                attribute.PropertyInfo = prop;

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

                attribute.PublicRelationshipName ??= _formatter.FormatPropertyName(prop);
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

        private Type GetResourceDefinitionType(Type entityType) => typeof(ResourceDefinition<>).MakeGenericType(entityType);

        private void AssertEntityIsNotAlreadyDefined(Type entityType)
        {
            if (_resources.Any(e => e.ResourceType == entityType))
                throw new InvalidOperationException($"Cannot add entity type {entityType} to context resourceGraph, there is already an entity of that type configured.");
        }
    }
}
