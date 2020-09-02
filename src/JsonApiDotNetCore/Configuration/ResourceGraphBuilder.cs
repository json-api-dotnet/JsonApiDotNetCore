using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Configuration
{
    public class ResourceGraphBuilder
    {
        private readonly ILogger<ResourceGraphBuilder> _logger;
        private readonly IJsonApiOptions _options;
        private readonly List<ResourceContext> _resources = new List<ResourceContext>();

        public ResourceGraphBuilder(IJsonApiOptions options, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            
            _logger = loggerFactory.CreateLogger<ResourceGraphBuilder>();
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Constructs the <see cref="ResourceGraph"/>.
        /// </summary>
        public IResourceGraph Build()
        {
            _resources.ForEach(SetResourceLinksOptions);
            return new ResourceGraph(_resources);
        }

        private void SetResourceLinksOptions(ResourceContext resourceContext)
        {
            var attribute = (ResourceLinksAttribute)resourceContext.ResourceType.GetCustomAttribute(typeof(ResourceLinksAttribute));
            if (attribute != null)
            {
                resourceContext.RelationshipLinks = attribute.RelationshipLinks;
                resourceContext.ResourceLinks = attribute.ResourceLinks;
                resourceContext.TopLevelLinks = attribute.TopLevelLinks;
            }
        }

        /// <summary>
        /// Adds a json:api resource to the resource graph.
        /// </summary>
        /// <typeparam name="TResource">The resource type.</typeparam>
        /// <typeparam name="TId">The identifier type of the resource</typeparam>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. 
        /// If nothing is specified, the configured casing convention formatter will be applied.
        /// </param>
        public ResourceGraphBuilder Add<TResource, TId>(string publicName = null) where TResource : class, IIdentifiable<TId>
            => Add(typeof(TResource), typeof(TId), publicName);
        
        /// <summary>
        /// <see cref="Add{TResource,TId}(string)"/>  
        /// </summary>
        public ResourceGraphBuilder Add<TResource>(string publicName = null) where TResource : class, IIdentifiable<int>
            => Add<TResource, int>(publicName);
        

        /// <summary>
        /// <see cref="Add{TResource,TId}(string)"/>  
        /// </summary>
        public ResourceGraphBuilder Add(Type resourceType, Type idType = null, string publicName = null)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            if (_resources.All(e => e.ResourceType != resourceType))
            {
                if (TypeHelper.IsOrImplementsInterface(resourceType, typeof(IIdentifiable)))
                {
                    publicName ??= FormatResourceName(resourceType);
                    idType ??= TypeLocator.GetIdType(resourceType);
                    var resourceContext = CreateResourceContext(publicName, resourceType, idType);
                    _resources.Add(resourceContext);
                }
                else
                {
                    _logger.LogWarning($"Entity '{resourceType}' does not implement '{nameof(IIdentifiable)}'.");
                }
            }

            return this;
        }

        private ResourceContext CreateResourceContext(string publicName, Type resourceType, Type idType) => new ResourceContext
        {
            ResourceName = publicName,
            ResourceType = resourceType,
            IdentityType = idType,
            Attributes = GetAttributes(resourceType),
            Relationships = GetRelationships(resourceType),
            EagerLoads = GetEagerLoads(resourceType),
            ResourceDefinitionType = GetResourceDefinitionType(resourceType)
        };

        private IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            var attributes = new List<AttrAttribute>();

            foreach (var property in resourceType.GetProperties())
            {
                var attribute = (AttrAttribute)property.GetCustomAttribute(typeof(AttrAttribute));

                // Although strictly not correct, 'id' is added to the list of attributes for convenience.
                // For example, it enables to filter on ID, without the need to special-case existing logic.
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

        private IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

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
                        throw new InvalidConfigurationException($"Invalid {nameof(HasManyThroughAttribute)} on '{resourceType}.{attribute.Property.Name}': Resource does not contain a property named '{hasManyThroughAttribute.ThroughPropertyName}'.");

                    var throughType = TryGetThroughType(throughProperty);
                    if (throughType == null)
                        throw new InvalidConfigurationException($"Invalid {nameof(HasManyThroughAttribute)} on '{resourceType}.{attribute.Property.Name}': Referenced property '{throughProperty.Name}' does not implement 'ICollection<T>'.");

                    // ICollection<ArticleTag>
                    hasManyThroughAttribute.ThroughProperty = throughProperty;

                    // ArticleTag
                    hasManyThroughAttribute.ThroughType = throughType;

                    var throughProperties = throughType.GetProperties();

                    // ArticleTag.Article
                    hasManyThroughAttribute.LeftProperty = throughProperties.SingleOrDefault(x => x.PropertyType == resourceType)
                        ?? throw new InvalidConfigurationException($"{throughType} does not contain a navigation property to type {resourceType}");

                    // ArticleTag.ArticleId
                    var leftIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.LeftProperty.Name);
                    hasManyThroughAttribute.LeftIdProperty = throughProperties.SingleOrDefault(x => x.Name == leftIdPropertyName)
                        ?? throw new InvalidConfigurationException($"{throughType} does not contain a relationship ID property to type {resourceType} with name {leftIdPropertyName}");

                    // ArticleTag.Tag
                    hasManyThroughAttribute.RightProperty = throughProperties.SingleOrDefault(x => x.PropertyType == hasManyThroughAttribute.RightType)
                        ?? throw new InvalidConfigurationException($"{throughType} does not contain a navigation property to type {hasManyThroughAttribute.RightType}");

                    // ArticleTag.TagId
                    var rightIdPropertyName = JsonApiOptions.RelatedIdMapper.GetRelatedIdPropertyName(hasManyThroughAttribute.RightProperty.Name);
                    hasManyThroughAttribute.RightIdProperty = throughProperties.SingleOrDefault(x => x.Name == rightIdPropertyName)
                        ?? throw new InvalidConfigurationException($"{throughType} does not contain a relationship ID property to type {hasManyThroughAttribute.RightType} with name {rightIdPropertyName}");
                }
            }

            return attributes;
        }

        private Type TryGetThroughType(PropertyInfo throughProperty)
        {
            if (throughProperty.PropertyType.IsGenericType)
            {
                var typeArguments = throughProperty.PropertyType.GetGenericArguments();
                if (typeArguments.Length == 1)
                {
                    var constructedThroughType = typeof(ICollection<>).MakeGenericType(typeArguments[0]);
                    if (TypeHelper.IsOrImplementsInterface(throughProperty.PropertyType, constructedThroughType))
                    {
                        return typeArguments[0];
                    }
                }
            }

            return null;
        }

        private Type GetRelationshipType(RelationshipAttribute relationship, PropertyInfo property)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (property == null) throw new ArgumentNullException(nameof(property));

            return relationship is HasOneAttribute ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
        }

        private IReadOnlyCollection<EagerLoadAttribute> GetEagerLoads(Type resourceType, int recursionDepth = 0)
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

        private Type TypeOrElementType(Type type)
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
