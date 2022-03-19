using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Provides helper methods to resolve a chain of fields (relationships and attributes) from the resource graph.
/// </summary>
internal sealed class ResourceFieldChainResolver
{
    private static readonly ResourceFieldChainErrorFormatter ErrorFormatter = new();

    /// <summary>
    /// Resolves a chain of to-one relationships.
    /// <example>author</example>
    /// <example>
    /// author.address.country
    /// </example>
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChain(ResourceType resourceType, string path,
        Action<ResourceFieldAttribute, ResourceType, string>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();
        ResourceType nextResourceType = resourceType;

        foreach (string publicName in path.Split("."))
        {
            RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path, FieldChainInheritanceRequirement.Disabled);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
        }

        return chainBuilder.ToImmutable();
    }

    /// <summary>
    /// Resolves a chain of relationships that ends in a to-many relationship, for example: blogs.owner.articles.comments
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveToManyChain(ResourceType resourceType, string path,
        Action<ResourceFieldAttribute, ResourceType, string>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;

        foreach (string publicName in publicNameParts[..^1])
        {
            RelationshipAttribute relationship = GetRelationship(publicName, nextResourceType, path, FieldChainInheritanceRequirement.Disabled);

            validateCallback?.Invoke(relationship, nextResourceType, path);

            chainBuilder.Add(relationship);
            nextResourceType = relationship.RightType;
        }

        string lastName = publicNameParts[^1];
        RelationshipAttribute lastToManyRelationship = GetToManyRelationship(lastName, nextResourceType, path, FieldChainInheritanceRequirement.Disabled);

        validateCallback?.Invoke(lastToManyRelationship, nextResourceType, path);

        chainBuilder.Add(lastToManyRelationship);
        return chainBuilder.ToImmutable();
    }

    /// <summary>
    /// Resolves a chain of relationships.
    /// <example>
    /// blogs.articles.comments
    /// </example>
    /// <example>
    /// author.address
    /// </example>
    /// <example>
    /// articles.revisions.author
    /// </example>
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveRelationshipChain(ResourceType resourceType, string path,
        Action<RelationshipAttribute, ResourceType, string>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();
        ResourceType nextResourceType = resourceType;

        foreach (string publicName in path.Split("."))
        {
            RelationshipAttribute relationship = GetRelationship(publicName, nextResourceType, path, FieldChainInheritanceRequirement.Disabled);

            validateCallback?.Invoke(relationship, nextResourceType, path);

            chainBuilder.Add(relationship);
            nextResourceType = relationship.RightType;
        }

        return chainBuilder.ToImmutable();
    }

    /// <summary>
    /// Resolves a chain of to-one relationships that ends in an attribute.
    /// <example>
    /// author.address.country.name
    /// </example>
    /// <example>name</example>
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInAttribute(ResourceType resourceType, string path,
        FieldChainInheritanceRequirement inheritanceRequirement, Action<ResourceFieldAttribute, ResourceType, string>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;

        foreach (string publicName in publicNameParts[..^1])
        {
            RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path, inheritanceRequirement);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
        }

        string lastName = publicNameParts[^1];
        AttrAttribute lastAttribute = GetAttribute(lastName, nextResourceType, path, inheritanceRequirement);

        validateCallback?.Invoke(lastAttribute, nextResourceType, path);

        chainBuilder.Add(lastAttribute);
        return chainBuilder.ToImmutable();
    }

    /// <summary>
    /// Resolves a chain of to-one relationships that ends in a to-many relationship.
    /// <example>
    /// article.comments
    /// </example>
    /// <example>
    /// comments
    /// </example>
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInToMany(ResourceType resourceType, string path,
        FieldChainInheritanceRequirement inheritanceRequirement, Action<ResourceFieldAttribute, ResourceType, string>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;

        foreach (string publicName in publicNameParts[..^1])
        {
            RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path, inheritanceRequirement);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
        }

        string lastName = publicNameParts[^1];

        RelationshipAttribute toManyRelationship = GetToManyRelationship(lastName, nextResourceType, path, inheritanceRequirement);

        validateCallback?.Invoke(toManyRelationship, nextResourceType, path);

        chainBuilder.Add(toManyRelationship);
        return chainBuilder.ToImmutable();
    }

    /// <summary>
    /// Resolves a chain of to-one relationships that ends in either an attribute or a to-one relationship.
    /// <example>
    /// author.address.country.name
    /// </example>
    /// <example>
    /// author.address
    /// </example>
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInAttributeOrToOne(ResourceType resourceType, string path,
        Action<ResourceFieldAttribute, ResourceType, string>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;

        foreach (string publicName in publicNameParts[..^1])
        {
            RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path, FieldChainInheritanceRequirement.Disabled);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
        }

        string lastName = publicNameParts[^1];
        ResourceFieldAttribute lastField = GetField(lastName, nextResourceType, path);

        if (lastField is HasManyAttribute)
        {
            string message = ErrorFormatter.GetForWrongFieldType(ResourceFieldCategory.Field, lastName, path, nextResourceType,
                "an attribute or a to-one relationship");

            throw new QueryParseException(message);
        }

        validateCallback?.Invoke(lastField, nextResourceType, path);

        chainBuilder.Add(lastField);
        return chainBuilder.ToImmutable();
    }

    private RelationshipAttribute GetRelationship(string publicName, ResourceType resourceType, string path,
        FieldChainInheritanceRequirement inheritanceRequirement)
    {
        IReadOnlyCollection<RelationshipAttribute> relationships = inheritanceRequirement == FieldChainInheritanceRequirement.Disabled
            ? resourceType.FindRelationshipByPublicName(publicName)?.AsArray() ?? Array.Empty<RelationshipAttribute>()
            : resourceType.GetRelationshipsInTypeOrDerived(publicName);

        if (relationships.Count == 0)
        {
            string message = ErrorFormatter.GetForNotFound(ResourceFieldCategory.Relationship, publicName, path, resourceType, inheritanceRequirement);
            throw new QueryParseException(message);
        }

        if (inheritanceRequirement == FieldChainInheritanceRequirement.RequireSingleMatch && relationships.Count > 1)
        {
            string message = ErrorFormatter.GetForMultipleMatches(ResourceFieldCategory.Relationship, publicName, path);
            throw new QueryParseException(message);
        }

        return relationships.First();
    }

    private RelationshipAttribute GetToManyRelationship(string publicName, ResourceType resourceType, string path,
        FieldChainInheritanceRequirement inheritanceRequirement)
    {
        RelationshipAttribute relationship = GetRelationship(publicName, resourceType, path, inheritanceRequirement);

        if (relationship is not HasManyAttribute)
        {
            string message = ErrorFormatter.GetForWrongFieldType(ResourceFieldCategory.Relationship, publicName, path, resourceType, "a to-many relationship");
            throw new QueryParseException(message);
        }

        return relationship;
    }

    private RelationshipAttribute GetToOneRelationship(string publicName, ResourceType resourceType, string path,
        FieldChainInheritanceRequirement inheritanceRequirement)
    {
        RelationshipAttribute relationship = GetRelationship(publicName, resourceType, path, inheritanceRequirement);

        if (relationship is not HasOneAttribute)
        {
            string message = ErrorFormatter.GetForWrongFieldType(ResourceFieldCategory.Relationship, publicName, path, resourceType, "a to-one relationship");
            throw new QueryParseException(message);
        }

        return relationship;
    }

    private AttrAttribute GetAttribute(string publicName, ResourceType resourceType, string path, FieldChainInheritanceRequirement inheritanceRequirement)
    {
        IReadOnlyCollection<AttrAttribute> attributes = inheritanceRequirement == FieldChainInheritanceRequirement.Disabled
            ? resourceType.FindAttributeByPublicName(publicName)?.AsArray() ?? Array.Empty<AttrAttribute>()
            : resourceType.GetAttributesInTypeOrDerived(publicName);

        if (attributes.Count == 0)
        {
            string message = ErrorFormatter.GetForNotFound(ResourceFieldCategory.Attribute, publicName, path, resourceType, inheritanceRequirement);
            throw new QueryParseException(message);
        }

        if (inheritanceRequirement == FieldChainInheritanceRequirement.RequireSingleMatch && attributes.Count > 1)
        {
            string message = ErrorFormatter.GetForMultipleMatches(ResourceFieldCategory.Attribute, publicName, path);
            throw new QueryParseException(message);
        }

        return attributes.First();
    }

    public ResourceFieldAttribute GetField(string publicName, ResourceType resourceType, string path)
    {
        ResourceFieldAttribute? field = resourceType.Fields.FirstOrDefault(nextField => nextField.PublicName == publicName);

        if (field == null)
        {
            string message = ErrorFormatter.GetForNotFound(ResourceFieldCategory.Field, publicName, path, resourceType,
                FieldChainInheritanceRequirement.Disabled);

            throw new QueryParseException(message);
        }

        return field;
    }
}
