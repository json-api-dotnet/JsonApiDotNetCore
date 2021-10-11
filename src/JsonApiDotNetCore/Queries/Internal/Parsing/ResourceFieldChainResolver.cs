#nullable disable

using System;
using System.Collections.Immutable;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    /// <summary>
    /// Provides helper methods to resolve a chain of fields (relationships and attributes) from the resource graph.
    /// </summary>
    internal sealed class ResourceFieldChainResolver
    {
        /// <summary>
        /// Resolves a chain of relationships that ends in a to-many relationship, for example: blogs.owner.articles.comments
        /// </summary>
        public IImmutableList<ResourceFieldAttribute> ResolveToManyChain(ResourceType resourceType, string path,
            Action<ResourceFieldAttribute, ResourceType, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceType nextResourceType = resourceType;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute relationship = GetRelationship(publicName, nextResourceType, path);

                validateCallback?.Invoke(relationship, nextResourceType, path);

                chainBuilder.Add(relationship);
                nextResourceType = relationship.RightType;
            }

            string lastName = publicNameParts[^1];
            RelationshipAttribute lastToManyRelationship = GetToManyRelationship(lastName, nextResourceType, path);

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
            Action<RelationshipAttribute, ResourceType, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();
            ResourceType nextResourceType = resourceType;

            foreach (string publicName in path.Split("."))
            {
                RelationshipAttribute relationship = GetRelationship(publicName, nextResourceType, path);

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
            Action<ResourceFieldAttribute, ResourceType, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceType nextResourceType = resourceType;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path);

                validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

                chainBuilder.Add(toOneRelationship);
                nextResourceType = toOneRelationship.RightType;
            }

            string lastName = publicNameParts[^1];
            AttrAttribute lastAttribute = GetAttribute(lastName, nextResourceType, path);

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
            Action<ResourceFieldAttribute, ResourceType, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceType nextResourceType = resourceType;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path);

                validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

                chainBuilder.Add(toOneRelationship);
                nextResourceType = toOneRelationship.RightType;
            }

            string lastName = publicNameParts[^1];

            RelationshipAttribute toManyRelationship = GetToManyRelationship(lastName, nextResourceType, path);

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
            Action<ResourceFieldAttribute, ResourceType, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceType nextResourceType = resourceType;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceType, path);

                validateCallback?.Invoke(toOneRelationship, nextResourceType, path);

                chainBuilder.Add(toOneRelationship);
                nextResourceType = toOneRelationship.RightType;
            }

            string lastName = publicNameParts[^1];
            ResourceFieldAttribute lastField = GetField(lastName, nextResourceType, path);

            if (lastField is HasManyAttribute)
            {
                throw new QueryParseException(path == lastName
                    ? $"Field '{lastName}' must be an attribute or a to-one relationship on resource type '{nextResourceType.PublicName}'."
                    : $"Field '{lastName}' in '{path}' must be an attribute or a to-one relationship on resource type '{nextResourceType.PublicName}'.");
            }

            validateCallback?.Invoke(lastField, nextResourceType, path);

            chainBuilder.Add(lastField);
            return chainBuilder.ToImmutable();
        }

        private RelationshipAttribute GetRelationship(string publicName, ResourceType resourceType, string path)
        {
            RelationshipAttribute relationship = resourceType.FindRelationshipByPublicName(publicName);

            if (relationship == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' does not exist on resource type '{resourceType.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' does not exist on resource type '{resourceType.PublicName}'.");
            }

            return relationship;
        }

        private RelationshipAttribute GetToManyRelationship(string publicName, ResourceType resourceType, string path)
        {
            RelationshipAttribute relationship = GetRelationship(publicName, resourceType, path);

            if (relationship is not HasManyAttribute)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' must be a to-many relationship on resource type '{resourceType.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' must be a to-many relationship on resource type '{resourceType.PublicName}'.");
            }

            return relationship;
        }

        private RelationshipAttribute GetToOneRelationship(string publicName, ResourceType resourceType, string path)
        {
            RelationshipAttribute relationship = GetRelationship(publicName, resourceType, path);

            if (relationship is not HasOneAttribute)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' must be a to-one relationship on resource type '{resourceType.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' must be a to-one relationship on resource type '{resourceType.PublicName}'.");
            }

            return relationship;
        }

        private AttrAttribute GetAttribute(string publicName, ResourceType resourceType, string path)
        {
            AttrAttribute attribute = resourceType.FindAttributeByPublicName(publicName);

            if (attribute == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Attribute '{publicName}' does not exist on resource type '{resourceType.PublicName}'."
                    : $"Attribute '{publicName}' in '{path}' does not exist on resource type '{resourceType.PublicName}'.");
            }

            return attribute;
        }

        public ResourceFieldAttribute GetField(string publicName, ResourceType resourceType, string path)
        {
            ResourceFieldAttribute field = resourceType.Fields.FirstOrDefault(nextField => nextField.PublicName == publicName);

            if (field == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Field '{publicName}' does not exist on resource type '{resourceType.PublicName}'."
                    : $"Field '{publicName}' in '{path}' does not exist on resource type '{resourceType.PublicName}'.");
            }

            return field;
        }
    }
}
