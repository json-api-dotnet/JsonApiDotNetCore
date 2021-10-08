using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Builds and configures the <see cref="ResourceGraph" />.
    /// </summary>
    [PublicAPI]
    public class ResourceGraphBuilder
    {
        private readonly IJsonApiOptions _options;
        private readonly ILogger<ResourceGraphBuilder> _logger;
        private readonly HashSet<ResourceType> _resourceTypes = new();
        private readonly TypeLocator _typeLocator = new();

        public ResourceGraphBuilder(IJsonApiOptions options, ILoggerFactory loggerFactory)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));

            _options = options;
            _logger = loggerFactory.CreateLogger<ResourceGraphBuilder>();
        }

        /// <summary>
        /// Constructs the <see cref="ResourceGraph" />.
        /// </summary>
        public IResourceGraph Build()
        {
            var resourceGraph = new ResourceGraph(_resourceTypes);

            foreach (RelationshipAttribute relationship in _resourceTypes.SelectMany(resourceType => resourceType.Relationships))
            {
                relationship.LeftType = resourceGraph.GetResourceType(relationship.LeftClrType);
                relationship.RightType = resourceGraph.GetResourceType(relationship.RightClrType);
            }

            return resourceGraph;
        }

        public ResourceGraphBuilder Add(DbContext dbContext)
        {
            ArgumentGuard.NotNull(dbContext, nameof(dbContext));

            foreach (IEntityType entityType in dbContext.Model.GetEntityTypes())
            {
                if (!IsImplicitManyToManyJoinEntity(entityType))
                {
                    Add(entityType.ClrType);
                }
            }

            return this;
        }

        private static bool IsImplicitManyToManyJoinEntity(IEntityType entityType)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            return entityType is EntityType { IsImplicitlyCreatedJoinEntityType: true };
#pragma warning restore EF1001 // Internal EF Core API usage.
        }

        /// <summary>
        /// Adds a JSON:API resource with <code>int</code> as the identifier CLR type.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource CLR type.
        /// </typeparam>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. If nothing is specified, the naming convention is applied on the pluralized CLR
        /// type name.
        /// </param>
        public ResourceGraphBuilder Add<TResource>(string publicName = null)
            where TResource : class, IIdentifiable<int>
        {
            return Add<TResource, int>(publicName);
        }

        /// <summary>
        /// Adds a JSON:API resource.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource CLR type.
        /// </typeparam>
        /// <typeparam name="TId">
        /// The resource identifier CLR type.
        /// </typeparam>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. If nothing is specified, the naming convention is applied on the pluralized CLR
        /// type name.
        /// </param>
        public ResourceGraphBuilder Add<TResource, TId>(string publicName = null)
            where TResource : class, IIdentifiable<TId>
        {
            return Add(typeof(TResource), typeof(TId), publicName);
        }

        /// <summary>
        /// Adds a JSON:API resource.
        /// </summary>
        /// <param name="resourceClrType">
        /// The resource CLR type.
        /// </param>
        /// <param name="idClrType">
        /// The resource identifier CLR type.
        /// </param>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. If nothing is specified, the naming convention is applied on the pluralized CLR
        /// type name.
        /// </param>
        public ResourceGraphBuilder Add(Type resourceClrType, Type idClrType = null, string publicName = null)
        {
            ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

            if (_resourceTypes.Any(resourceType => resourceType.ClrType == resourceClrType))
            {
                return this;
            }

            if (resourceClrType.IsOrImplementsInterface(typeof(IIdentifiable)))
            {
                string effectivePublicName = publicName ?? FormatResourceName(resourceClrType);
                Type effectiveIdType = idClrType ?? _typeLocator.TryGetIdType(resourceClrType);

                if (effectiveIdType == null)
                {
                    throw new InvalidConfigurationException($"Resource type '{resourceClrType}' implements 'IIdentifiable', but not 'IIdentifiable<TId>'.");
                }

                ResourceType resourceType = CreateResourceType(effectivePublicName, resourceClrType, effectiveIdType);
                _resourceTypes.Add(resourceType);
            }
            else
            {
                _logger.LogWarning($"Skipping: Type '{resourceClrType}' does not implement '{nameof(IIdentifiable)}'.");
            }

            return this;
        }

        private ResourceType CreateResourceType(string publicName, Type resourceClrType, Type idClrType)
        {
            IReadOnlyCollection<AttrAttribute> attributes = GetAttributes(resourceClrType);
            IReadOnlyCollection<RelationshipAttribute> relationships = GetRelationships(resourceClrType);
            IReadOnlyCollection<EagerLoadAttribute> eagerLoads = GetEagerLoads(resourceClrType);

            var linksAttribute = (ResourceLinksAttribute)resourceClrType.GetCustomAttribute(typeof(ResourceLinksAttribute));

            return linksAttribute == null
                ? new ResourceType(publicName, resourceClrType, idClrType, attributes, relationships, eagerLoads)
                : new ResourceType(publicName, resourceClrType, idClrType, attributes, relationships, eagerLoads, linksAttribute.TopLevelLinks,
                    linksAttribute.ResourceLinks, linksAttribute.RelationshipLinks);
        }

        private IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceClrType)
        {
            var attributes = new List<AttrAttribute>();

            foreach (PropertyInfo property in resourceClrType.GetProperties())
            {
                var attribute = (AttrAttribute)property.GetCustomAttribute(typeof(AttrAttribute));

                // Although strictly not correct, 'id' is added to the list of attributes for convenience.
                // For example, it enables to filter on ID, without the need to special-case existing logic.
                // And when using sparse fields, it silently adds 'id' to the set of attributes to retrieve.
                if (property.Name == nameof(Identifiable<object>.Id) && attribute == null)
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
                {
                    continue;
                }

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

        private IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type resourceClrType)
        {
            var relationships = new List<RelationshipAttribute>();
            PropertyInfo[] properties = resourceClrType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var relationship = (RelationshipAttribute)property.GetCustomAttribute(typeof(RelationshipAttribute));

                if (relationship != null)
                {
                    relationship.Property = property;
                    relationship.PublicName ??= FormatPropertyName(property);
                    relationship.LeftClrType = resourceClrType;
                    relationship.RightClrType = GetRelationshipType(relationship, property);

                    relationships.Add(relationship);
                }
            }

            return relationships;
        }

        private Type GetRelationshipType(RelationshipAttribute relationship, PropertyInfo property)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(property, nameof(property));

            return relationship is HasOneAttribute ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
        }

        private IReadOnlyCollection<EagerLoadAttribute> GetEagerLoads(Type resourceClrType, int recursionDepth = 0)
        {
            AssertNoInfiniteRecursion(recursionDepth);

            var attributes = new List<EagerLoadAttribute>();
            PropertyInfo[] properties = resourceClrType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var attribute = (EagerLoadAttribute)property.GetCustomAttribute(typeof(EagerLoadAttribute));

                if (attribute == null)
                {
                    continue;
                }

                Type innerType = TypeOrElementType(property.PropertyType);
                attribute.Children = GetEagerLoads(innerType, recursionDepth + 1);
                attribute.Property = property;

                attributes.Add(attribute);
            }

            return attributes;
        }

        [AssertionMethod]
        private static void AssertNoInfiniteRecursion(int recursionDepth)
        {
            if (recursionDepth >= 500)
            {
                throw new InvalidOperationException("Infinite recursion detected in eager-load chain.");
            }
        }

        private Type TypeOrElementType(Type type)
        {
            Type[] interfaces = type.GetInterfaces()
                .Where(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToArray();

            return interfaces.Length == 1 ? interfaces.Single().GenericTypeArguments[0] : type;
        }

        private string FormatResourceName(Type resourceClrType)
        {
            var formatter = new ResourceNameFormatter(_options.SerializerOptions.PropertyNamingPolicy);
            return formatter.FormatResourceName(resourceClrType);
        }

        private string FormatPropertyName(PropertyInfo resourceProperty)
        {
            return _options.SerializerOptions.PropertyNamingPolicy == null
                ? resourceProperty.Name
                : _options.SerializerOptions.PropertyNamingPolicy.ConvertName(resourceProperty.Name);
        }
    }
}
