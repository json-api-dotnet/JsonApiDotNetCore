using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Metadata about the shape of a JSON:API resource in the resource graph.
/// </summary>
[PublicAPI]
public sealed class ResourceType
{
    private static readonly IReadOnlySet<ResourceType> EmptyResourceTypeSet = new HashSet<ResourceType>().AsReadOnly();
    private static readonly IReadOnlySet<AttrAttribute> EmptyAttributeSet = new HashSet<AttrAttribute>().AsReadOnly();
    private static readonly IReadOnlySet<RelationshipAttribute> EmptyRelationshipSet = new HashSet<RelationshipAttribute>().AsReadOnly();

    private readonly Dictionary<string, ResourceFieldAttribute> _fieldsByPublicName = [];
    private readonly Dictionary<string, ResourceFieldAttribute> _fieldsByPropertyName = [];
    private readonly Lazy<IReadOnlySet<ResourceType>> _lazyAllConcreteDerivedTypes;

    /// <summary>
    /// The publicly exposed resource name.
    /// </summary>
    public string PublicName { get; }

    /// <summary>
    /// Whether API clients are allowed or required to provide IDs when creating resources of this type. When <c>null</c>, the value from global options
    /// applies.
    /// </summary>
    public ClientIdGenerationMode? ClientIdGeneration { get; }

    /// <summary>
    /// The CLR type of the resource.
    /// </summary>
    public Type ClrType { get; }

    /// <summary>
    /// The CLR type of the resource identity.
    /// </summary>
    public Type IdentityClrType { get; }

    /// <summary>
    /// The base resource type, in case this is a derived type.
    /// </summary>
    public ResourceType? BaseType { get; internal set; }

    /// <summary>
    /// The resource types that directly derive from this one.
    /// </summary>
    public IReadOnlySet<ResourceType> DirectlyDerivedTypes { get; internal set; } = EmptyResourceTypeSet;

    /// <summary>
    /// Exposed resource attributes and relationships. See https://jsonapi.org/format/#document-resource-object-fields. When using resource inheritance, this
    /// includes the attributes and relationships from base types.
    /// </summary>
    public IReadOnlyCollection<ResourceFieldAttribute> Fields { get; }

    /// <summary>
    /// Exposed resource attributes. See https://jsonapi.org/format/#document-resource-object-attributes. When using resource inheritance, this includes the
    /// attributes from base types.
    /// </summary>
    public IReadOnlyCollection<AttrAttribute> Attributes { get; }

    /// <summary>
    /// Exposed resource relationships. See https://jsonapi.org/format/#document-resource-object-relationships. When using resource inheritance, this
    /// includes the relationships from base types.
    /// </summary>
    public IReadOnlyCollection<RelationshipAttribute> Relationships { get; }

    /// <summary>
    /// Related entities that are not exposed as resource relationships. When using resource inheritance, this includes the eager-loads from base types.
    /// </summary>
    public IReadOnlyCollection<EagerLoadAttribute> EagerLoads { get; }

    /// <summary>
    /// Configures which links to write in the top-level links object for this resource type. Defaults to <see cref="LinkTypes.NotConfigured" />, which falls
    /// back to TopLevelLinks in global options.
    /// </summary>
    /// <remarks>
    /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.TopLevelLinks" /> usage.
    /// </remarks>
    public LinkTypes TopLevelLinks { get; }

    /// <summary>
    /// Configures which links to write in the resource-level links object for this resource type. Defaults to <see cref="LinkTypes.NotConfigured" />, which
    /// falls back to ResourceLinks in global options.
    /// </summary>
    /// <remarks>
    /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.ResourceLinks" /> usage.
    /// </remarks>
    public LinkTypes ResourceLinks { get; }

    /// <summary>
    /// Configures which links to write in the relationship-level links object for all relationships of this resource type. Defaults to
    /// <see cref="LinkTypes.NotConfigured" />, which falls back to RelationshipLinks in global options. This can be overruled per relationship by setting
    /// <see cref="RelationshipAttribute.Links" />.
    /// </summary>
    /// <remarks>
    /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.RelationshipLinks" /> usage.
    /// </remarks>
    public LinkTypes RelationshipLinks { get; }

    public ResourceType(string publicName, ClientIdGenerationMode? clientIdGeneration, Type clrType, Type identityClrType,
        LinkTypes topLevelLinks = LinkTypes.NotConfigured, LinkTypes resourceLinks = LinkTypes.NotConfigured,
        LinkTypes relationshipLinks = LinkTypes.NotConfigured)
        : this(publicName, clientIdGeneration, clrType, identityClrType, null, null, null, topLevelLinks, resourceLinks, relationshipLinks)
    {
    }

    public ResourceType(string publicName, ClientIdGenerationMode? clientIdGeneration, Type clrType, Type identityClrType,
        IReadOnlyCollection<AttrAttribute>? attributes, IReadOnlyCollection<RelationshipAttribute>? relationships,
        IReadOnlyCollection<EagerLoadAttribute>? eagerLoads, LinkTypes topLevelLinks = LinkTypes.NotConfigured,
        LinkTypes resourceLinks = LinkTypes.NotConfigured, LinkTypes relationshipLinks = LinkTypes.NotConfigured)
    {
        ArgumentGuard.NotNullNorEmpty(publicName);
        ArgumentGuard.NotNull(clrType);
        ArgumentGuard.NotNull(identityClrType);

        PublicName = publicName;
        ClientIdGeneration = clientIdGeneration;
        ClrType = clrType;
        IdentityClrType = identityClrType;
        Attributes = attributes ?? Array.Empty<AttrAttribute>();
        Relationships = relationships ?? Array.Empty<RelationshipAttribute>();
        EagerLoads = eagerLoads ?? Array.Empty<EagerLoadAttribute>();
        TopLevelLinks = topLevelLinks;
        ResourceLinks = resourceLinks;
        RelationshipLinks = relationshipLinks;
        Fields = Attributes.Cast<ResourceFieldAttribute>().Concat(Relationships).ToArray().AsReadOnly();

        foreach (ResourceFieldAttribute field in Fields)
        {
            _fieldsByPublicName.Add(field.PublicName, field);
            _fieldsByPropertyName.Add(field.Property.Name, field);
        }

        _lazyAllConcreteDerivedTypes = new Lazy<IReadOnlySet<ResourceType>>(ResolveAllConcreteDerivedTypes, LazyThreadSafetyMode.PublicationOnly);
    }

    private IReadOnlySet<ResourceType> ResolveAllConcreteDerivedTypes()
    {
        HashSet<ResourceType> allConcreteDerivedTypes = [];
        AddConcreteDerivedTypes(this, allConcreteDerivedTypes);

        return allConcreteDerivedTypes.AsReadOnly();
    }

    private static void AddConcreteDerivedTypes(ResourceType resourceType, ISet<ResourceType> allConcreteDerivedTypes)
    {
        foreach (ResourceType derivedType in resourceType.DirectlyDerivedTypes)
        {
            if (!derivedType.ClrType.IsAbstract)
            {
                allConcreteDerivedTypes.Add(derivedType);
            }

            AddConcreteDerivedTypes(derivedType, allConcreteDerivedTypes);
        }
    }

    public AttrAttribute GetAttributeByPublicName(string publicName)
    {
        AttrAttribute? attribute = FindAttributeByPublicName(publicName);
        return attribute ?? throw new InvalidOperationException($"Attribute '{publicName}' does not exist on resource type '{PublicName}'.");
    }

    public AttrAttribute? FindAttributeByPublicName(string publicName)
    {
        ArgumentGuard.NotNull(publicName);

        return _fieldsByPublicName.TryGetValue(publicName, out ResourceFieldAttribute? field) && field is AttrAttribute attribute ? attribute : null;
    }

    public AttrAttribute GetAttributeByPropertyName(string propertyName)
    {
        AttrAttribute? attribute = FindAttributeByPropertyName(propertyName);

        return attribute ?? throw new InvalidOperationException($"Attribute for property '{propertyName}' does not exist on resource type '{ClrType.Name}'.");
    }

    public AttrAttribute? FindAttributeByPropertyName(string propertyName)
    {
        ArgumentGuard.NotNull(propertyName);

        return _fieldsByPropertyName.TryGetValue(propertyName, out ResourceFieldAttribute? field) && field is AttrAttribute attribute ? attribute : null;
    }

    public RelationshipAttribute GetRelationshipByPublicName(string publicName)
    {
        RelationshipAttribute? relationship = FindRelationshipByPublicName(publicName);
        return relationship ?? throw new InvalidOperationException($"Relationship '{publicName}' does not exist on resource type '{PublicName}'.");
    }

    public RelationshipAttribute? FindRelationshipByPublicName(string publicName)
    {
        ArgumentGuard.NotNull(publicName);

        return _fieldsByPublicName.TryGetValue(publicName, out ResourceFieldAttribute? field) && field is RelationshipAttribute relationship
            ? relationship
            : null;
    }

    public RelationshipAttribute GetRelationshipByPropertyName(string propertyName)
    {
        RelationshipAttribute? relationship = FindRelationshipByPropertyName(propertyName);

        return relationship ??
            throw new InvalidOperationException($"Relationship for property '{propertyName}' does not exist on resource type '{ClrType.Name}'.");
    }

    public RelationshipAttribute? FindRelationshipByPropertyName(string propertyName)
    {
        ArgumentGuard.NotNull(propertyName);

        return _fieldsByPropertyName.TryGetValue(propertyName, out ResourceFieldAttribute? field) && field is RelationshipAttribute relationship
            ? relationship
            : null;
    }

    /// <summary>
    /// Returns all directly and indirectly non-abstract resource types that derive from this resource type.
    /// </summary>
    public IReadOnlySet<ResourceType> GetAllConcreteDerivedTypes()
    {
        return _lazyAllConcreteDerivedTypes.Value;
    }

    /// <summary>
    /// Searches the tree of derived types to find a match for the specified <paramref name="clrType" />.
    /// </summary>
    public ResourceType GetTypeOrDerived(Type clrType)
    {
        ArgumentGuard.NotNull(clrType);

        ResourceType? derivedType = FindTypeOrDerived(this, clrType);

        if (derivedType == null)
        {
            throw new InvalidOperationException($"Resource type '{PublicName}' is not a base type of '{clrType}'.");
        }

        return derivedType;
    }

    private static ResourceType? FindTypeOrDerived(ResourceType type, Type clrType)
    {
        if (type.ClrType == clrType)
        {
            return type;
        }

        foreach (ResourceType derivedType in type.DirectlyDerivedTypes)
        {
            ResourceType? matchingType = FindTypeOrDerived(derivedType, clrType);

            if (matchingType != null)
            {
                return matchingType;
            }
        }

        return null;
    }

    internal IReadOnlySet<AttrAttribute> GetAttributesInTypeOrDerived(string publicName)
    {
        if (IsPartOfTypeHierarchy())
        {
            return GetAttributesInTypeOrDerived(this, publicName);
        }

        AttrAttribute? attribute = FindAttributeByPublicName(publicName);

        if (attribute == null)
        {
            return EmptyAttributeSet;
        }

        HashSet<AttrAttribute> attributes = [attribute];
        return attributes.AsReadOnly();
    }

    private static IReadOnlySet<AttrAttribute> GetAttributesInTypeOrDerived(ResourceType resourceType, string publicName)
    {
        AttrAttribute? attribute = resourceType.FindAttributeByPublicName(publicName);

        if (attribute != null)
        {
            HashSet<AttrAttribute> attributes = [attribute];
            return attributes.AsReadOnly();
        }

        // Hiding base members using the 'new' keyword instead of 'override' (effectively breaking inheritance) is currently not supported.
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords
        HashSet<AttrAttribute> attributesInDerivedTypes = [];

        foreach (AttrAttribute attributeInDerivedType in resourceType.DirectlyDerivedTypes
            .Select(derivedType => GetAttributesInTypeOrDerived(derivedType, publicName)).SelectMany(attributesInDerivedType => attributesInDerivedType))
        {
            attributesInDerivedTypes.Add(attributeInDerivedType);
        }

        return attributesInDerivedTypes.AsReadOnly();
    }

    internal IReadOnlySet<RelationshipAttribute> GetRelationshipsInTypeOrDerived(string publicName)
    {
        if (IsPartOfTypeHierarchy())
        {
            return GetRelationshipsInTypeOrDerived(this, publicName);
        }

        RelationshipAttribute? relationship = FindRelationshipByPublicName(publicName);

        if (relationship == null)
        {
            return EmptyRelationshipSet;
        }

        HashSet<RelationshipAttribute> relationships = [relationship];
        return relationships.AsReadOnly();
    }

    private static IReadOnlySet<RelationshipAttribute> GetRelationshipsInTypeOrDerived(ResourceType resourceType, string publicName)
    {
        RelationshipAttribute? relationship = resourceType.FindRelationshipByPublicName(publicName);

        if (relationship != null)
        {
            HashSet<RelationshipAttribute> relationships = [relationship];
            return relationships.AsReadOnly();
        }

        // Hiding base members using the 'new' keyword instead of 'override' (effectively breaking inheritance) is currently not supported.
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords
        HashSet<RelationshipAttribute> relationshipsInDerivedTypes = [];

        foreach (RelationshipAttribute relationshipInDerivedType in resourceType.DirectlyDerivedTypes
            .Select(derivedType => GetRelationshipsInTypeOrDerived(derivedType, publicName))
            .SelectMany(relationshipsInDerivedType => relationshipsInDerivedType))
        {
            relationshipsInDerivedTypes.Add(relationshipInDerivedType);
        }

        return relationshipsInDerivedTypes.AsReadOnly();
    }

    internal bool IsPartOfTypeHierarchy()
    {
        return BaseType != null || DirectlyDerivedTypes.Count > 0;
    }

    public override string ToString()
    {
        return PublicName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (ResourceType)obj;

        return PublicName == other.PublicName && ClrType == other.ClrType && IdentityClrType == other.IdentityClrType &&
            Attributes.SequenceEqual(other.Attributes) && Relationships.SequenceEqual(other.Relationships) && EagerLoads.SequenceEqual(other.EagerLoads) &&
            TopLevelLinks == other.TopLevelLinks && ResourceLinks == other.ResourceLinks && RelationshipLinks == other.RelationshipLinks;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(PublicName);
        hashCode.Add(ClrType);
        hashCode.Add(IdentityClrType);

        foreach (AttrAttribute attribute in Attributes)
        {
            hashCode.Add(attribute);
        }

        foreach (RelationshipAttribute relationship in Relationships)
        {
            hashCode.Add(relationship);
        }

        foreach (EagerLoadAttribute eagerLoad in EagerLoads)
        {
            hashCode.Add(eagerLoad);
        }

        hashCode.Add(TopLevelLinks);
        hashCode.Add(ResourceLinks);
        hashCode.Add(RelationshipLinks);

        return hashCode.ToHashCode();
    }
}
