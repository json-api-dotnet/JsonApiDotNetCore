using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Builders
{
    public interface IResourceGraphBuilder
    {
        /// <summary>
        /// Construct the <see cref="ResourceGraph"/>
        /// </summary>
        IResourceGraph Build();

        /// <summary>
        /// Add a json:api resource
        /// </summary>
        /// <typeparam name="TResource">The resource model type</typeparam>
        /// <param name="pluralizedTypeName">
        /// The pluralized name that should be exposed by the API. 
        /// If nothing is specified, the configured name formatter will be used.
        /// See <see cref="JsonApiOptions.ResourceNameFormatter" />.
        /// </param>
        IResourceGraphBuilder AddResource<TResource>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<int>;

        /// <summary>
        /// Add a json:api resource
        /// </summary>
        /// <typeparam name="TResource">The resource model type</typeparam>
        /// <typeparam name="TId">The resource model identifier type</typeparam>
        /// <param name="pluralizedTypeName">
        /// The pluralized name that should be exposed by the API. 
        /// If nothing is specified, the configured name formatter will be used.
        /// See <see cref="JsonApiOptions.ResourceNameFormatter" />.
        /// </param>
        IResourceGraphBuilder AddResource<TResource, TId>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<TId>;

        /// <summary>
        /// Add a json:api resource
        /// </summary>
        /// <param name="entityType">The resource model type</param>
        /// <param name="idType">The resource model identifier type</param>
        /// <param name="pluralizedTypeName">
        /// The pluralized name that should be exposed by the API. 
        /// If nothing is specified, the configured name formatter will be used.
        /// See <see cref="JsonApiOptions.ResourceNameFormatter" />.
        /// </param>
        IResourceGraphBuilder AddResource(Type entityType, Type idType, string pluralizedTypeName = null);

        /// <summary>
        /// Add all the models that are part of the provided <see cref="DbContext" /> 
        /// that also implement <see cref="IIdentifiable"/>
        /// </summary>
        /// <typeparam name="T">The <see cref="DbContext"/> implementation type.</typeparam>
        IResourceGraphBuilder AddDbContext<T>() where T : DbContext;

        /// <summary>
        /// Specify the <see cref="IResourceNameFormatter"/> used to format resource names.
        /// </summary>
        /// <param name="resourceNameFormatter">Formatter used to define exposed resource names by convention.</param>
        IResourceGraphBuilder UseNameFormatter(IResourceNameFormatter resourceNameFormatter);

        /// <summary>
        /// Which links to include. Defaults to <see cref="LinkTypes.All"/>.
        /// </summary>
        LinkTypes DocumentLinks { get; set; }
    }

    public class ResourceGraphBuilder : IResourceGraphBuilder
    {
        private List<ContextEntity> _entities = new List<ContextEntity>();
        private List<ValidationResult> _validationResults = new List<ValidationResult>();
        private bool _usesDbContext;
        private IResourceNameFormatter _resourceNameFormatter = JsonApiOptions.ResourceNameFormatter;

        public LinkTypes DocumentLinks { get; set; } = LinkTypes.All;

        public IResourceGraph Build()
        {
            // this must be done at build so that call order doesn't matter
            _entities.ForEach(e => e.Links = GetLinkFlags(e.EntityType));

            var graph = new ResourceGraph(_entities, _usesDbContext, _validationResults);
            return graph;
        }

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource<TResource>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<int>
            => AddResource<TResource, int>(pluralizedTypeName);

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource<TResource, TId>(string pluralizedTypeName = null) where TResource : class, IIdentifiable<TId>
            => AddResource(typeof(TResource), typeof(TId), pluralizedTypeName);

        /// <inheritdoc />
        public IResourceGraphBuilder AddResource(Type entityType, Type idType, string pluralizedTypeName = null)
        {
            AssertEntityIsNotAlreadyDefined(entityType);

            pluralizedTypeName = pluralizedTypeName ?? _resourceNameFormatter.FormatResourceName(entityType);

            _entities.Add(GetEntity(pluralizedTypeName, entityType, idType));

            return this;
        }

        private ContextEntity GetEntity(string pluralizedTypeName, Type entityType, Type idType) => new ContextEntity
        {
            EntityName = pluralizedTypeName,
            EntityType = entityType,
            IdentityType = idType,
            Attributes = GetAttributes(entityType),
            Relationships = GetRelationships(entityType),
            ResourceType = GetResourceDefinitionType(entityType)
        };

        private LinkTypes GetLinkFlags(Type entityType)
        {
            var attribute = (LinksAttribute)entityType.GetTypeInfo().GetCustomAttribute(typeof(LinksAttribute));
            if (attribute != null)
                return attribute.Links;

            return DocumentLinks;
        }

        protected virtual List<AttrAttribute> GetAttributes(Type entityType)
        {
            var attributes = new List<AttrAttribute>();

            var properties = entityType.GetProperties();

            foreach (var prop in properties)
            {
                if (prop.Name == nameof(Identifiable<int>.Id))
                {
                    var idAttr = new AttrAttribute()
                    {
                        PublicAttributeName = JsonApiOptions.ResourceNameFormatter.FormatPropertyName(prop),
                        PropertyInfo = prop,
                        InternalAttributeName = prop.Name
                    };
                    attributes.Add(idAttr);
                    continue;
                }

                var attribute = (AttrAttribute)prop.GetCustomAttribute(typeof(AttrAttribute));
                if (attribute == null)
                    continue;

                attribute.PublicAttributeName = attribute.PublicAttributeName ?? JsonApiOptions.ResourceNameFormatter.FormatPropertyName(prop);
                attribute.InternalAttributeName = prop.Name;
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

                attribute.PublicRelationshipName = attribute.PublicRelationshipName ?? JsonApiOptions.ResourceNameFormatter.FormatPropertyName(prop);
                attribute.InternalRelationshipName = prop.Name;
                attribute.Type = GetRelationshipType(attribute, prop);
                attributes.Add(attribute);

                if (attribute is HasManyThroughAttribute hasManyThroughAttribute) {
                    var throughProperty = properties.SingleOrDefault(p => p.Name == hasManyThroughAttribute.InternalThroughName);
                    if(throughProperty == null)
                        throw new JsonApiSetupException($"Invalid '{nameof(HasManyThroughAttribute)}' on type '{entityType}'. Type does not contain a property named '{hasManyThroughAttribute.InternalThroughName}'.");
                    
                    if(throughProperty.PropertyType.Implements<IList>() == false)
                        throw new JsonApiSetupException($"Invalid '{nameof(HasManyThroughAttribute)}' on type '{entityType}.{throughProperty.Name}'. Property type does not implement IList.");
                    
                    // assumption: the property should be a generic collection, e.g. List<ArticleTag>
                    if(throughProperty.PropertyType.IsGenericType == false)
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
                    hasManyThroughAttribute.RightProperty = throughProperties.SingleOrDefault(x => x.PropertyType == hasManyThroughAttribute.Type)
                        ?? throw new JsonApiSetupException($"{hasManyThroughAttribute.ThroughType} does not contain a navigation property to type {hasManyThroughAttribute.Type}");
                    
                    // ArticleTag.TagId
                    var rightIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.RightProperty.Name);
                    hasManyThroughAttribute.RightIdProperty = throughProperties.SingleOrDefault(x => x.Name == rightIdPropertyName)
                        ?? throw new JsonApiSetupException($"{hasManyThroughAttribute.ThroughType} does not contain a relationship id property to type {hasManyThroughAttribute.Type} with name {rightIdPropertyName}");
                }
            }

            return attributes;
        }

        protected virtual Type GetRelationshipType(RelationshipAttribute relation, PropertyInfo prop) =>
            relation.IsHasMany ? prop.PropertyType.GetGenericArguments()[0] : prop.PropertyType;

        private Type GetResourceDefinitionType(Type entityType) => typeof(ResourceDefinition<>).MakeGenericType(entityType);

        /// <inheritdoc />
        public IResourceGraphBuilder AddDbContext<T>() where T : DbContext
        {
            _usesDbContext = true;

            var contextType = typeof(T);

            var contextProperties = contextType.GetProperties();

            foreach (var property in contextProperties)
            {
                var dbSetType = property.PropertyType;

                if (dbSetType.GetTypeInfo().IsGenericType
                    && dbSetType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = dbSetType.GetGenericArguments()[0];

                    AssertEntityIsNotAlreadyDefined(entityType);

                    var (isJsonApiResource, idType) = GetIdType(entityType);

                    if (isJsonApiResource)
                        _entities.Add(GetEntity(GetResourceNameFromDbSetProperty(property, entityType), entityType, idType));
                }
            }

            return this;
        }

        private string GetResourceNameFromDbSetProperty(PropertyInfo property, Type resourceType)
        {
            // this check is actually duplicated in the DefaultResourceNameFormatter
            // however, we perform it here so that we allow class attributes to be prioritized over
            // the DbSet attribute. Eventually, the DbSet attribute should be deprecated.
            //
            // check the class definition first
            // [Resource("models"] public class Model : Identifiable { /* ... */ }
            if (resourceType.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute classResourceAttribute)
                return classResourceAttribute.ResourceName;

            // check the DbContext member next
            // [Resource("models")] public DbSet<Model> Models { get; set; }
            if (property.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute resourceAttribute)
                return resourceAttribute.ResourceName;

            // fallback to the established convention using the DbSet Property.Name
            // e.g DbSet<FooBar> FooBars { get; set; } => "foo-bars"
            return _resourceNameFormatter.ApplyCasingConvention(property.Name);
        }

        private (bool isJsonApiResource, Type idType) GetIdType(Type resourceType)
        {
            var possible = TypeLocator.GetIdType(resourceType);
            if (possible.isJsonApiResource)
                return possible;

            _validationResults.Add(new ValidationResult(LogLevel.Warning, $"{resourceType} does not implement 'IIdentifiable<>'. "));

            return (false, null);
        }

        private void AssertEntityIsNotAlreadyDefined(Type entityType)
        {
            if (_entities.Any(e => e.EntityType == entityType))
                throw new InvalidOperationException($"Cannot add entity type {entityType} to context graph, there is already an entity of that type configured.");
        }

        /// <inheritdoc />
        public IResourceGraphBuilder UseNameFormatter(IResourceNameFormatter resourceNameFormatter)
        {
            _resourceNameFormatter = resourceNameFormatter;
            return this;
        }
    }
}
