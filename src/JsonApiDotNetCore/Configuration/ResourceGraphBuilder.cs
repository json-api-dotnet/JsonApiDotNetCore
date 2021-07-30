using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
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
        private readonly List<ResourceContext> _resources = new();
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
        /// Adds a JSON:API resource with <code>int</code> as the identifier type.
        /// </summary>
        /// <typeparam name="TResource">
        /// The resource model type.
        /// </typeparam>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. If nothing is specified, the configured naming convention formatter will be
        /// applied.
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
        /// The resource model type.
        /// </typeparam>
        /// <typeparam name="TId">
        /// The resource model identifier type.
        /// </typeparam>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. If nothing is specified, the configured naming convention formatter will be
        /// applied.
        /// </param>
        public ResourceGraphBuilder Add<TResource, TId>(string publicName = null)
            where TResource : class, IIdentifiable<TId>
        {
            return Add(typeof(TResource), typeof(TId), publicName);
        }

        /// <summary>
        /// Adds a JSON:API resource.
        /// </summary>
        /// <param name="resourceType">
        /// The resource model type.
        /// </param>
        /// <param name="idType">
        /// The resource model identifier type.
        /// </param>
        /// <param name="publicName">
        /// The name under which the resource is publicly exposed by the API. If nothing is specified, the configured naming convention formatter will be
        /// applied.
        /// </param>
        public ResourceGraphBuilder Add(Type resourceType, Type idType = null, string publicName = null)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (_resources.Any(resourceContext => resourceContext.ResourceType == resourceType))
            {
                return this;
            }

            if (resourceType.IsOrImplementsInterface(typeof(IIdentifiable)))
            {
                string effectivePublicName = publicName ?? FormatResourceName(resourceType);
                Type effectiveIdType = idType ?? _typeLocator.TryGetIdType(resourceType);

                ResourceContext resourceContext = CreateResourceContext(effectivePublicName, resourceType, effectiveIdType);
                _resources.Add(resourceContext);
            }
            else
            {
                _logger.LogWarning($"Entity '{resourceType}' does not implement '{nameof(IIdentifiable)}'.");
            }

            return this;
        }

        private ResourceContext CreateResourceContext(string publicName, Type resourceType, Type idType)
        {
            return new()
            {
                PublicName = publicName,
                ResourceType = resourceType,
                IdentityType = idType,
                Attributes = GetAttributes(resourceType),
                Relationships = GetRelationships(resourceType),
                EagerLoads = GetEagerLoads(resourceType)
            };
        }

        private IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceType)
        {
            var attributes = new List<AttrAttribute>();

            foreach (PropertyInfo property in resourceType.GetProperties())
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

        private IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type resourceType)
        {
            var attributes = new List<RelationshipAttribute>();
            PropertyInfo[] properties = resourceType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var attribute = (RelationshipAttribute)property.GetCustomAttribute(typeof(RelationshipAttribute));

                if (attribute != null)
                {
                    attribute.Property = property;
                    attribute.PublicName ??= FormatPropertyName(property);
                    attribute.LeftType = resourceType;
                    attribute.RightType = GetRelationshipType(attribute, property);

                    attributes.Add(attribute);
                }
            }

            return attributes;
        }

        private Type TryGetThroughType(PropertyInfo throughProperty)
        {
            if (throughProperty.PropertyType.IsGenericType)
            {
                Type[] typeArguments = throughProperty.PropertyType.GetGenericArguments();

                if (typeArguments.Length == 1)
                {
                    Type constructedThroughType = typeof(ICollection<>).MakeGenericType(typeArguments[0]);

                    if (throughProperty.PropertyType.IsOrImplementsInterface(constructedThroughType))
                    {
                        return typeArguments[0];
                    }
                }
            }

            return null;
        }

        private Type GetRelationshipType(RelationshipAttribute relationship, PropertyInfo property)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(property, nameof(property));

            return relationship is HasOneAttribute ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];
        }

        private IReadOnlyCollection<EagerLoadAttribute> GetEagerLoads(Type resourceType, int recursionDepth = 0)
        {
            AssertNoInfiniteRecursion(recursionDepth);

            var attributes = new List<EagerLoadAttribute>();
            PropertyInfo[] properties = resourceType.GetProperties();

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

        private string FormatResourceName(Type resourceType)
        {
            var formatter = new ResourceNameFormatter(_options.SerializerNamingStrategy);
            return formatter.FormatResourceName(resourceType);
        }

        private string FormatPropertyName(PropertyInfo resourceProperty)
        {
            return _options.SerializerNamingStrategy.GetPropertyName(resourceProperty.Name, false);
        }
    }
}
