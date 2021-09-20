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
        private readonly IResourceGraph _resourceGraph;

        public ResourceFieldChainResolver(IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            _resourceGraph = resourceGraph;
        }

        /// <summary>
        /// Resolves a chain of relationships that ends in a to-many relationship, for example: blogs.owner.articles.comments
        /// </summary>
        public IImmutableList<ResourceFieldAttribute> ResolveToManyChain(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceContext nextResourceContext = resourceContext;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute relationship = GetRelationship(publicName, nextResourceContext, path);

                validateCallback?.Invoke(relationship, nextResourceContext, path);

                chainBuilder.Add(relationship);
                nextResourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            }

            string lastName = publicNameParts[^1];
            RelationshipAttribute lastToManyRelationship = GetToManyRelationship(lastName, nextResourceContext, path);

            validateCallback?.Invoke(lastToManyRelationship, nextResourceContext, path);

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
        public IImmutableList<ResourceFieldAttribute> ResolveRelationshipChain(ResourceContext resourceContext, string path,
            Action<RelationshipAttribute, ResourceContext, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();
            ResourceContext nextResourceContext = resourceContext;

            foreach (string publicName in path.Split("."))
            {
                RelationshipAttribute relationship = GetRelationship(publicName, nextResourceContext, path);

                validateCallback?.Invoke(relationship, nextResourceContext, path);

                chainBuilder.Add(relationship);
                nextResourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
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
        public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInAttribute(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceContext nextResourceContext = resourceContext;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceContext, path);

                validateCallback?.Invoke(toOneRelationship, nextResourceContext, path);

                chainBuilder.Add(toOneRelationship);
                nextResourceContext = _resourceGraph.GetResourceContext(toOneRelationship.RightType);
            }

            string lastName = publicNameParts[^1];
            AttrAttribute lastAttribute = GetAttribute(lastName, nextResourceContext, path);

            validateCallback?.Invoke(lastAttribute, nextResourceContext, path);

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
        public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInToMany(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceContext nextResourceContext = resourceContext;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceContext, path);

                validateCallback?.Invoke(toOneRelationship, nextResourceContext, path);

                chainBuilder.Add(toOneRelationship);
                nextResourceContext = _resourceGraph.GetResourceContext(toOneRelationship.RightType);
            }

            string lastName = publicNameParts[^1];

            RelationshipAttribute toManyRelationship = GetToManyRelationship(lastName, nextResourceContext, path);

            validateCallback?.Invoke(toManyRelationship, nextResourceContext, path);

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
        public IImmutableList<ResourceFieldAttribute> ResolveToOneChainEndingInAttributeOrToOne(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder = ImmutableArray.CreateBuilder<ResourceFieldAttribute>();

            string[] publicNameParts = path.Split(".");
            ResourceContext nextResourceContext = resourceContext;

            foreach (string publicName in publicNameParts[..^1])
            {
                RelationshipAttribute toOneRelationship = GetToOneRelationship(publicName, nextResourceContext, path);

                validateCallback?.Invoke(toOneRelationship, nextResourceContext, path);

                chainBuilder.Add(toOneRelationship);
                nextResourceContext = _resourceGraph.GetResourceContext(toOneRelationship.RightType);
            }

            string lastName = publicNameParts[^1];
            ResourceFieldAttribute lastField = GetField(lastName, nextResourceContext, path);

            if (lastField is HasManyAttribute)
            {
                throw new QueryParseException(path == lastName
                    ? $"Field '{lastName}' must be an attribute or a to-one relationship on resource '{nextResourceContext.PublicName}'."
                    : $"Field '{lastName}' in '{path}' must be an attribute or a to-one relationship on resource '{nextResourceContext.PublicName}'.");
            }

            validateCallback?.Invoke(lastField, nextResourceContext, path);

            chainBuilder.Add(lastField);
            return chainBuilder.ToImmutable();
        }

        private RelationshipAttribute GetRelationship(string publicName, ResourceContext resourceContext, string path)
        {
            RelationshipAttribute relationship = resourceContext.TryGetRelationshipByPublicName(publicName);

            if (relationship == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' does not exist on resource '{resourceContext.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' does not exist on resource '{resourceContext.PublicName}'.");
            }

            return relationship;
        }

        private RelationshipAttribute GetToManyRelationship(string publicName, ResourceContext resourceContext, string path)
        {
            RelationshipAttribute relationship = GetRelationship(publicName, resourceContext, path);

            if (relationship is not HasManyAttribute)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' must be a to-many relationship on resource '{resourceContext.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' must be a to-many relationship on resource '{resourceContext.PublicName}'.");
            }

            return relationship;
        }

        private RelationshipAttribute GetToOneRelationship(string publicName, ResourceContext resourceContext, string path)
        {
            RelationshipAttribute relationship = GetRelationship(publicName, resourceContext, path);

            if (relationship is not HasOneAttribute)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' must be a to-one relationship on resource '{resourceContext.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' must be a to-one relationship on resource '{resourceContext.PublicName}'.");
            }

            return relationship;
        }

        private AttrAttribute GetAttribute(string publicName, ResourceContext resourceContext, string path)
        {
            AttrAttribute attribute = resourceContext.TryGetAttributeByPublicName(publicName);

            if (attribute == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Attribute '{publicName}' does not exist on resource '{resourceContext.PublicName}'."
                    : $"Attribute '{publicName}' in '{path}' does not exist on resource '{resourceContext.PublicName}'.");
            }

            return attribute;
        }

        public ResourceFieldAttribute GetField(string publicName, ResourceContext resourceContext, string path)
        {
            ResourceFieldAttribute field = resourceContext.Fields.FirstOrDefault(nextField => nextField.PublicName == publicName);

            if (field == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Field '{publicName}' does not exist on resource '{resourceContext.PublicName}'."
                    : $"Field '{publicName}' in '{path}' does not exist on resource '{resourceContext.PublicName}'.");
            }

            return field;
        }
    }
}
