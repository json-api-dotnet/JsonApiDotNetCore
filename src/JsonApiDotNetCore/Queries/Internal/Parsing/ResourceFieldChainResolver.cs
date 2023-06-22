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
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChain(ResourceType resourceType, string path, int position,
        Action<ResourceFieldAttribute, ResourceType, int>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();
        ResourceType nextResourceType = resourceType;
        int fieldOffset = 0;

        foreach (string publicName in path.Split("."))
        {
            fieldOffset += fieldOffset > 0 ? 1 : 0;

            RelationshipAttribute toOneRelationship =
                GetToOneRelationship(publicName, nextResourceType, FieldChainInheritanceRequirement.Disabled, position + fieldOffset);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, position + fieldOffset);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
            fieldOffset += publicName.Length;
        }

        return chainBuilder.ToImmutable();
    }

    /// <summary>
    /// Resolves a chain of relationships that ends in a to-many relationship, for example: blogs.owner.articles.comments
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> ResolveToManyChain(ResourceType resourceType, string path, int position,
        Action<ResourceFieldAttribute, ResourceType, int>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;
        int fieldOffset = 0;

        foreach (string publicName in publicNameParts[..^1])
        {
            fieldOffset += fieldOffset > 0 ? 1 : 0;

            RelationshipAttribute relationship =
                GetRelationship(publicName, nextResourceType, FieldChainInheritanceRequirement.Disabled, position + fieldOffset);

            validateCallback?.Invoke(relationship, nextResourceType, position + fieldOffset);

            chainBuilder.Add(relationship);
            nextResourceType = relationship.RightType;
            fieldOffset += publicName.Length;
        }

        string lastName = publicNameParts[^1];
        fieldOffset += fieldOffset > 0 ? 1 : 0;

        RelationshipAttribute lastToManyRelationship =
            GetToManyRelationship(lastName, nextResourceType, FieldChainInheritanceRequirement.Disabled, position + fieldOffset);

        validateCallback?.Invoke(lastToManyRelationship, nextResourceType, position + fieldOffset);

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
    public IImmutableList<ResourceFieldAttribute> ResolveRelationshipChain(ResourceType resourceType, string path, int position,
        Action<RelationshipAttribute, ResourceType, int>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();
        ResourceType nextResourceType = resourceType;
        int fieldOffset = 0;

        foreach (string publicName in path.Split("."))
        {
            fieldOffset += fieldOffset > 0 ? 1 : 0;

            RelationshipAttribute relationship =
                GetRelationship(publicName, nextResourceType, FieldChainInheritanceRequirement.Disabled, position + fieldOffset);

            validateCallback?.Invoke(relationship, nextResourceType, position + fieldOffset);

            chainBuilder.Add(relationship);
            nextResourceType = relationship.RightType;
            fieldOffset += publicName.Length;
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
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInAttribute(ResourceType resourceType, string path, int position,
        FieldChainInheritanceRequirement inheritanceRequirement, Action<ResourceFieldAttribute, ResourceType, int>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;
        int fieldOffset = 0;

        foreach (string publicName in publicNameParts[..^1])
        {
            fieldOffset += fieldOffset > 0 ? 1 : 0;
            RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, inheritanceRequirement, position + fieldOffset);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, position + fieldOffset);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
            fieldOffset += publicName.Length;
        }

        string lastName = publicNameParts[^1];
        fieldOffset += fieldOffset > 0 ? 1 : 0;
        AttrAttribute lastAttribute = GetAttribute(lastName, nextResourceType, inheritanceRequirement, position + fieldOffset);

        validateCallback?.Invoke(lastAttribute, nextResourceType, position + fieldOffset);

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
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInToMany(ResourceType resourceType, string path, int position,
        FieldChainInheritanceRequirement inheritanceRequirement, Action<ResourceFieldAttribute, ResourceType, int>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;
        int fieldOffset = 0;

        foreach (string publicName in publicNameParts[..^1])
        {
            fieldOffset += fieldOffset > 0 ? 1 : 0;
            RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, inheritanceRequirement, position + fieldOffset);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, position + fieldOffset);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
            fieldOffset += publicName.Length;
        }

        string lastName = publicNameParts[^1];
        fieldOffset += fieldOffset > 0 ? 1 : 0;
        RelationshipAttribute toManyRelationship = GetToManyRelationship(lastName, nextResourceType, inheritanceRequirement, position + fieldOffset);

        validateCallback?.Invoke(toManyRelationship, nextResourceType, position + fieldOffset);

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
    public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInAttributeOrToOne(ResourceType resourceType, string path, int position,
        Action<ResourceFieldAttribute, ResourceType, int>? validateCallback = null)
    {
        ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

        string[] publicNameParts = path.Split(".");
        ResourceType nextResourceType = resourceType;
        int fieldOffset = 0;

        foreach (string publicName in publicNameParts[..^1])
        {
            fieldOffset += fieldOffset > 0 ? 1 : 0;

            RelationshipAttribute toOneRelationship =
                GetToOneRelationship(publicName, nextResourceType, FieldChainInheritanceRequirement.Disabled, position + fieldOffset);

            validateCallback?.Invoke(toOneRelationship, nextResourceType, position + fieldOffset);

            chainBuilder.Add(toOneRelationship);
            nextResourceType = toOneRelationship.RightType;
            fieldOffset += publicName.Length;
        }

        string lastName = publicNameParts[^1];
        fieldOffset += fieldOffset > 0 ? 1 : 0;
        ResourceFieldAttribute lastField = GetField(lastName, nextResourceType, position + fieldOffset);

        if (lastField is HasManyAttribute)
        {
            string message = ErrorFormatter.GetForWrongFieldType(ResourceFieldCategory.Field, lastName, nextResourceType,
                "an attribute or a to-one relationship");

            throw new QueryParseException(message, position + fieldOffset);
        }

        validateCallback?.Invoke(lastField, nextResourceType, position + fieldOffset);

        chainBuilder.Add(lastField);
        return chainBuilder.ToImmutable();
    }

    private RelationshipAttribute GetRelationship(string publicName, ResourceType resourceType, FieldChainInheritanceRequirement inheritanceRequirement,
        int position)
    {
        IReadOnlyCollection<RelationshipAttribute> relationships = inheritanceRequirement == FieldChainInheritanceRequirement.Disabled
            ? resourceType.FindRelationshipByPublicName(publicName)?.AsArray() ?? Array.Empty<RelationshipAttribute>()
            : resourceType.GetRelationshipsInTypeOrDerived(publicName);

        if (relationships.Count == 0)
        {
            string message = ErrorFormatter.GetForNotFound(ResourceFieldCategory.Relationship, publicName, resourceType, inheritanceRequirement);
            throw new QueryParseException(message, position);
        }

        if (inheritanceRequirement == FieldChainInheritanceRequirement.RequireSingleMatch && relationships.Count > 1)
        {
            string message = ErrorFormatter.GetForMultipleMatches(ResourceFieldCategory.Relationship, publicName);
            throw new QueryParseException(message, position);
        }

        return relationships.First();
    }

    private RelationshipAttribute GetToManyRelationship(string publicName, ResourceType resourceType, FieldChainInheritanceRequirement inheritanceRequirement,
        int position)
    {
        RelationshipAttribute relationship = GetRelationship(publicName, resourceType, inheritanceRequirement, position);

        if (relationship is not HasManyAttribute)
        {
            string message = ErrorFormatter.GetForWrongFieldType(ResourceFieldCategory.Relationship, publicName, resourceType, "a to-many relationship");
            throw new QueryParseException(message, position);
        }

        return relationship;
    }

    private RelationshipAttribute GetToOneRelationship(string publicName, ResourceType resourceType, FieldChainInheritanceRequirement inheritanceRequirement,
        int position)
    {
        RelationshipAttribute relationship = GetRelationship(publicName, resourceType, inheritanceRequirement, position);

        if (relationship is not HasOneAttribute)
        {
            string message = ErrorFormatter.GetForWrongFieldType(ResourceFieldCategory.Relationship, publicName, resourceType, "a to-one relationship");
            throw new QueryParseException(message, position);
        }

        return relationship;
    }

    private AttrAttribute GetAttribute(string publicName, ResourceType resourceType, FieldChainInheritanceRequirement inheritanceRequirement, int position)
    {
        IReadOnlyCollection<AttrAttribute> attributes = inheritanceRequirement == FieldChainInheritanceRequirement.Disabled
            ? resourceType.FindAttributeByPublicName(publicName)?.AsArray() ?? Array.Empty<AttrAttribute>()
            : resourceType.GetAttributesInTypeOrDerived(publicName);

        if (attributes.Count == 0)
        {
            string message = ErrorFormatter.GetForNotFound(ResourceFieldCategory.Attribute, publicName, resourceType, inheritanceRequirement);
            throw new QueryParseException(message, position);
        }

        if (inheritanceRequirement == FieldChainInheritanceRequirement.RequireSingleMatch && attributes.Count > 1)
        {
            string message = ErrorFormatter.GetForMultipleMatches(ResourceFieldCategory.Attribute, publicName);
            throw new QueryParseException(message, position);
        }

        return attributes.First();
    }

    public ResourceFieldAttribute GetField(string publicName, ResourceType resourceType, int position)
    {
        ResourceFieldAttribute? field = resourceType.Fields.FirstOrDefault(nextField => nextField.PublicName == publicName);

        if (field == null)
        {
            string message = ErrorFormatter.GetForNotFound(ResourceFieldCategory.Field, publicName, resourceType, FieldChainInheritanceRequirement.Disabled);

            throw new QueryParseException(message, position);
        }

        return field;
    }
}
