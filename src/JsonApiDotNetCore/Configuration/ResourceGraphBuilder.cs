using System.Collections.ObjectModel;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Builds and configures the <see cref="ResourceGraph" />.
/// </summary>
[PublicAPI]
public partial class ResourceGraphBuilder
{
    private readonly IJsonApiOptions _options;
    private readonly ILogger<ResourceGraphBuilder> _logger;
    private readonly Dictionary<Type, ResourceType> _resourceTypesByClrType = [];
    private readonly TypeLocator _typeLocator = new();

    public ResourceGraphBuilder(IJsonApiOptions options, ILoggerFactory loggerFactory)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(loggerFactory);

        _options = options;
        _logger = loggerFactory.CreateLogger<ResourceGraphBuilder>();
    }

    /// <summary>
    /// Constructs the <see cref="ResourceGraph" />.
    /// </summary>
    public IResourceGraph Build()
    {
        IReadOnlySet<ResourceType> resourceTypes = _resourceTypesByClrType.Values.ToHashSet().AsReadOnly();

        if (resourceTypes.Count == 0)
        {
            LogResourceGraphIsEmpty();
        }

        var resourceGraph = new ResourceGraph(resourceTypes);

        SetFieldTypes(resourceGraph);
        SetRelationshipTypes(resourceGraph);
        SetDirectlyDerivedTypes(resourceGraph);
        ValidateFieldsInDerivedTypes(resourceGraph);

        return resourceGraph;
    }

    private static void SetFieldTypes(ResourceGraph resourceGraph)
    {
        foreach (ResourceFieldAttribute field in resourceGraph.GetResourceTypes().SelectMany(resourceType => resourceType.Fields))
        {
            field.Type = resourceGraph.GetResourceType(field.Property.ReflectedType!);
        }
    }

    private static void SetRelationshipTypes(ResourceGraph resourceGraph)
    {
        foreach (RelationshipAttribute relationship in resourceGraph.GetResourceTypes().SelectMany(resourceType => resourceType.Relationships))
        {
            Type rightClrType = relationship is HasOneAttribute
                ? relationship.Property.PropertyType
                : relationship.Property.PropertyType.GetGenericArguments()[0];

            ResourceType? rightType = resourceGraph.FindResourceType(rightClrType);

            if (rightType == null)
            {
                throw new InvalidConfigurationException($"Resource type '{relationship.LeftType.ClrType}' depends on " +
                    $"'{rightClrType}', which was not added to the resource graph.");
            }

            relationship.RightType = rightType;
        }
    }

    private static void SetDirectlyDerivedTypes(ResourceGraph resourceGraph)
    {
        Dictionary<ResourceType, HashSet<ResourceType>> directlyDerivedTypesPerBaseType = [];

        foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
        {
            ResourceType? baseType = resourceGraph.FindResourceType(resourceType.ClrType.BaseType!);

            if (baseType != null)
            {
                resourceType.BaseType = baseType;

                if (!directlyDerivedTypesPerBaseType.TryGetValue(baseType, out HashSet<ResourceType>? directlyDerivedTypes))
                {
                    directlyDerivedTypes = [];
                    directlyDerivedTypesPerBaseType[baseType] = directlyDerivedTypes;
                }

                directlyDerivedTypes.Add(resourceType);
            }
        }

        foreach ((ResourceType baseType, HashSet<ResourceType> directlyDerivedTypes) in directlyDerivedTypesPerBaseType)
        {
            if (directlyDerivedTypes.Count > 0)
            {
                baseType.DirectlyDerivedTypes = directlyDerivedTypes.AsReadOnly();
            }
        }
    }

    private void ValidateFieldsInDerivedTypes(ResourceGraph resourceGraph)
    {
        foreach (ResourceType resourceType in resourceGraph.GetResourceTypes())
        {
            if (resourceType.BaseType != null)
            {
                ValidateAttributesInDerivedType(resourceType);
                ValidateRelationshipsInDerivedType(resourceType);
            }
        }
    }

    private static void ValidateAttributesInDerivedType(ResourceType resourceType)
    {
        foreach (AttrAttribute attribute in resourceType.BaseType!.Attributes)
        {
            if (resourceType.FindAttributeByPublicName(attribute.PublicName) == null)
            {
                throw new InvalidConfigurationException($"Attribute '{attribute.PublicName}' from base type " +
                    $"'{resourceType.BaseType.ClrType}' does not exist in derived type '{resourceType.ClrType}'.");
            }
        }
    }

    private static void ValidateRelationshipsInDerivedType(ResourceType resourceType)
    {
        foreach (RelationshipAttribute relationship in resourceType.BaseType!.Relationships)
        {
            if (resourceType.FindRelationshipByPublicName(relationship.PublicName) == null)
            {
                throw new InvalidConfigurationException($"Relationship '{relationship.PublicName}' from base type " +
                    $"'{resourceType.BaseType.ClrType}' does not exist in derived type '{resourceType.ClrType}'.");
            }
        }
    }

    public ResourceGraphBuilder Add(DbContext dbContext)
    {
        ArgumentGuard.NotNull(dbContext);

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
        return entityType is { IsPropertyBag: true, HasSharedClrType: true };
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
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    public ResourceGraphBuilder Add<TResource, TId>(string? publicName = null)
        where TResource : class, IIdentifiable<TId>
#pragma warning restore AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
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
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    public ResourceGraphBuilder Add(Type resourceClrType, Type? idClrType = null, string? publicName = null)
#pragma warning restore AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    {
        ArgumentGuard.NotNull(resourceClrType);

        if (_resourceTypesByClrType.ContainsKey(resourceClrType))
        {
            return this;
        }

        if (resourceClrType.IsOrImplementsInterface<IIdentifiable>())
        {
            string effectivePublicName = publicName ?? FormatResourceName(resourceClrType);
            Type? effectiveIdType = idClrType ?? _typeLocator.LookupIdType(resourceClrType);

            if (effectiveIdType == null)
            {
                throw new InvalidConfigurationException($"Resource type '{resourceClrType}' implements 'IIdentifiable', but not 'IIdentifiable<TId>'.");
            }

            ResourceType resourceType = CreateResourceType(effectivePublicName, resourceClrType, effectiveIdType);

            AssertNoDuplicatePublicName(resourceType, effectivePublicName);

            _resourceTypesByClrType.Add(resourceClrType, resourceType);
        }
        else
        {
            if (resourceClrType.GetCustomAttribute<NoResourceAttribute>() == null)
            {
                LogResourceTypeDoesNotImplementInterface(resourceClrType, nameof(IIdentifiable));
            }
        }

        return this;
    }

    private ResourceType CreateResourceType(string publicName, Type resourceClrType, Type idClrType)
    {
        ClientIdGenerationMode? clientIdGeneration = GetClientIdGeneration(resourceClrType);

        Dictionary<string, AttrAttribute>.ValueCollection attributes = GetAttributes(resourceClrType);
        Dictionary<string, RelationshipAttribute>.ValueCollection relationships = GetRelationships(resourceClrType);
        ReadOnlyCollection<EagerLoadAttribute> eagerLoads = GetEagerLoads(resourceClrType);

        AssertNoDuplicatePublicName(attributes, relationships);

        var linksAttribute = resourceClrType.GetCustomAttribute<ResourceLinksAttribute>(true);

        return linksAttribute == null
            ? new ResourceType(publicName, clientIdGeneration, resourceClrType, idClrType, attributes, relationships, eagerLoads)
            : new ResourceType(publicName, clientIdGeneration, resourceClrType, idClrType, attributes, relationships, eagerLoads, linksAttribute.TopLevelLinks,
                linksAttribute.ResourceLinks, linksAttribute.RelationshipLinks);
    }

    private ClientIdGenerationMode? GetClientIdGeneration(Type resourceClrType)
    {
        var resourceAttribute = resourceClrType.GetCustomAttribute<ResourceAttribute>(true);
        return resourceAttribute?.NullableClientIdGeneration;
    }

    private Dictionary<string, AttrAttribute>.ValueCollection GetAttributes(Type resourceClrType)
    {
        var attributesByName = new Dictionary<string, AttrAttribute>();

        foreach (PropertyInfo property in resourceClrType.GetProperties())
        {
            var attribute = property.GetCustomAttribute<AttrAttribute>(true);

            if (attribute == null)
            {
                if (property.Name == nameof(Identifiable<object>.Id))
                {
                    // Although strictly not correct, 'id' is added to the list of attributes for convenience.
                    // For example, it enables to filter on ID, without the need to special-case existing logic.
                    // And when using sparse fieldsets, it silently adds 'id' to the set of attributes to retrieve.

                    attribute = new AttrAttribute();
                }
                else
                {
                    continue;
                }
            }

            SetPublicName(attribute, property);
            attribute.Property = property;

            if (!attribute.HasExplicitCapabilities)
            {
                attribute.Capabilities = _options.DefaultAttrCapabilities;
            }

            IncludeField(attributesByName, attribute);
        }

        if (attributesByName.Count < 2)
        {
            LogResourceTypeHasNoAttributes(resourceClrType);
        }

        return attributesByName.Values;
    }

    private Dictionary<string, RelationshipAttribute>.ValueCollection GetRelationships(Type resourceClrType)
    {
        var relationshipsByName = new Dictionary<string, RelationshipAttribute>();
        PropertyInfo[] properties = resourceClrType.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            var relationship = property.GetCustomAttribute<RelationshipAttribute>(true);

            if (relationship != null)
            {
                relationship.Property = property;
                SetPublicName(relationship, property);
                SetRelationshipCapabilities(relationship);

                IncludeField(relationshipsByName, relationship);
            }
        }

        return relationshipsByName.Values;
    }

    private void SetPublicName(ResourceFieldAttribute field, PropertyInfo property)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        field.PublicName ??= FormatPropertyName(property);
    }

    private void SetRelationshipCapabilities(RelationshipAttribute relationship)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        bool canInclude = relationship.CanInclude;
#pragma warning restore CS0618 // Type or member is obsolete

        if (relationship is HasOneAttribute hasOneRelationship)
        {
            SetHasOneRelationshipCapabilities(hasOneRelationship, canInclude);
        }
        else if (relationship is HasManyAttribute hasManyRelationship)
        {
            SetHasManyRelationshipCapabilities(hasManyRelationship, canInclude);
        }
    }

    private void SetHasOneRelationshipCapabilities(HasOneAttribute hasOneRelationship, bool canInclude)
    {
        if (!hasOneRelationship.HasExplicitCapabilities)
        {
            hasOneRelationship.Capabilities = _options.DefaultHasOneCapabilities;
        }

        if (hasOneRelationship.HasExplicitCanInclude)
        {
            hasOneRelationship.Capabilities = canInclude
                ? hasOneRelationship.Capabilities | HasOneCapabilities.AllowInclude
                : hasOneRelationship.Capabilities & ~HasOneCapabilities.AllowInclude;
        }
    }

    private void SetHasManyRelationshipCapabilities(HasManyAttribute hasManyRelationship, bool canInclude)
    {
        if (!hasManyRelationship.HasExplicitCapabilities)
        {
            hasManyRelationship.Capabilities = _options.DefaultHasManyCapabilities;
        }

        if (hasManyRelationship.HasExplicitCanInclude)
        {
            hasManyRelationship.Capabilities = canInclude
                ? hasManyRelationship.Capabilities | HasManyCapabilities.AllowInclude
                : hasManyRelationship.Capabilities & ~HasManyCapabilities.AllowInclude;
        }
    }

    private ReadOnlyCollection<EagerLoadAttribute> GetEagerLoads(Type resourceClrType, int recursionDepth = 0)
    {
        AssertNoInfiniteRecursion(recursionDepth);

        List<EagerLoadAttribute> eagerLoads = [];
        PropertyInfo[] properties = resourceClrType.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            var eagerLoad = property.GetCustomAttribute<EagerLoadAttribute>(true);

            if (eagerLoad == null)
            {
                continue;
            }

            Type innerType = TypeOrElementType(property.PropertyType);
            eagerLoad.Children = GetEagerLoads(innerType, recursionDepth + 1);
            eagerLoad.Property = property;

            eagerLoads.Add(eagerLoad);
        }

        return eagerLoads.AsReadOnly();
    }

    private static void IncludeField<TField>(Dictionary<string, TField> fieldsByName, TField field)
        where TField : ResourceFieldAttribute
    {
        if (fieldsByName.TryGetValue(field.PublicName, out TField? existingField))
        {
            throw CreateExceptionForDuplicatePublicName(field.Property.DeclaringType!, existingField, field);
        }

        fieldsByName.Add(field.PublicName, field);
    }

    private void AssertNoDuplicatePublicName(ResourceType resourceType, string effectivePublicName)
    {
        (Type? existingClrType, _) = _resourceTypesByClrType.FirstOrDefault(type => type.Value.PublicName == resourceType.PublicName);

        if (existingClrType != null)
        {
            throw new InvalidConfigurationException($"Resource '{existingClrType}' and '{resourceType.ClrType}' both use public name '{effectivePublicName}'.");
        }
    }

    private void AssertNoDuplicatePublicName(IReadOnlyCollection<AttrAttribute> attributes, IReadOnlyCollection<RelationshipAttribute> relationships)
    {
        IEnumerable<(AttrAttribute attribute, RelationshipAttribute relationship)> query =
            from attribute in attributes
            from relationship in relationships
            where attribute.PublicName == relationship.PublicName
            select (attribute, relationship);

        (AttrAttribute? duplicateAttribute, RelationshipAttribute? duplicateRelationship) = query.FirstOrDefault();

        if (duplicateAttribute != null && duplicateRelationship != null)
        {
            throw CreateExceptionForDuplicatePublicName(duplicateAttribute.Property.DeclaringType!, duplicateAttribute, duplicateRelationship);
        }
    }

    private static InvalidConfigurationException CreateExceptionForDuplicatePublicName(Type containingClrType, ResourceFieldAttribute existingField,
        ResourceFieldAttribute field)
    {
        return new InvalidConfigurationException(
            $"Properties '{containingClrType}.{existingField.Property.Name}' and '{containingClrType}.{field.Property.Name}' both use public name '{field.PublicName}'.");
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
        Type[] interfaces = type.GetInterfaces().Where(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .ToArray();

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

    [LoggerMessage(Level = LogLevel.Warning, Message = "The resource graph is empty.")]
    private partial void LogResourceGraphIsEmpty();

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Skipping: Type '{ResourceType}' does not implement '{InterfaceType}'. Add [NoResource] to suppress this warning.")]
    private partial void LogResourceTypeDoesNotImplementInterface(Type resourceType, string interfaceType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Type '{ResourceType}' does not contain any attributes.")]
    private partial void LogResourceTypeHasNoAttributes(Type resourceType);
}
