using System;
using System.Collections.Generic;
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
        private readonly IResourceContextProvider _resourceContextProvider;

        public ResourceFieldChainResolver(IResourceContextProvider resourceContextProvider)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        /// <summary>
        /// Resolves a chain of relationships that ends in a to-many relationship, for example: blogs.owner.articles.comments
        /// </summary>
        public IReadOnlyCollection<ResourceFieldAttribute> ResolveToManyChain(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            var chain = new List<ResourceFieldAttribute>();

            var publicNameParts = path.Split(".");

            foreach (string publicName in publicNameParts[..^1])
            {
                var relationship = GetRelationship(publicName, resourceContext, path);

                validateCallback?.Invoke(relationship, resourceContext, path);

                chain.Add(relationship);
                resourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);
            }

            string lastName = publicNameParts[^1];
            var lastToManyRelationship = GetToManyRelationship(lastName, resourceContext, path);

            validateCallback?.Invoke(lastToManyRelationship, resourceContext, path);

            chain.Add(lastToManyRelationship);
            return chain;
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
        public IReadOnlyCollection<ResourceFieldAttribute> ResolveRelationshipChain(ResourceContext resourceContext, string path, 
            Action<RelationshipAttribute, ResourceContext, string> validateCallback = null)
        {
            var chain = new List<ResourceFieldAttribute>();

            foreach (string publicName in path.Split("."))
            {
                var relationship = GetRelationship(publicName, resourceContext, path);

                validateCallback?.Invoke(relationship, resourceContext, path);

                chain.Add(relationship);
                resourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);
            }

            return chain;
        }

        /// <summary>
        /// Resolves a chain of to-one relationships that ends in an attribute.
        /// <example>
        /// author.address.country.name
        /// </example>
        /// <example>
        /// name
        /// </example>
        /// </summary>
        public IReadOnlyCollection<ResourceFieldAttribute> ResolveToOneChainEndingInAttribute(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            List<ResourceFieldAttribute> chain = new List<ResourceFieldAttribute>();

            var publicNameParts = path.Split(".");

            foreach (string publicName in publicNameParts[..^1])
            {
                var toOneRelationship = GetToOneRelationship(publicName, resourceContext, path);

                validateCallback?.Invoke(toOneRelationship, resourceContext, path);

                chain.Add(toOneRelationship);
                resourceContext = _resourceContextProvider.GetResourceContext(toOneRelationship.RightType);
            }

            string lastName = publicNameParts[^1];
            var lastAttribute = GetAttribute(lastName, resourceContext, path);

            validateCallback?.Invoke(lastAttribute, resourceContext, path);

            chain.Add(lastAttribute);
            return chain;
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
        public IReadOnlyCollection<ResourceFieldAttribute> ResolveToOneChainEndingInToMany(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            List<ResourceFieldAttribute> chain = new List<ResourceFieldAttribute>();

            var publicNameParts = path.Split(".");

            foreach (string publicName in publicNameParts[..^1])
            {
                var toOneRelationship = GetToOneRelationship(publicName, resourceContext, path);

                validateCallback?.Invoke(toOneRelationship, resourceContext, path);

                chain.Add(toOneRelationship);
                resourceContext = _resourceContextProvider.GetResourceContext(toOneRelationship.RightType);
            }

            string lastName = publicNameParts[^1];

            var toManyRelationship = GetToManyRelationship(lastName, resourceContext, path);

            validateCallback?.Invoke(toManyRelationship, resourceContext, path);

            chain.Add(toManyRelationship);
            return chain;
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
        public IReadOnlyCollection<ResourceFieldAttribute> ResolveToOneChainEndingInAttributeOrToOne(ResourceContext resourceContext, string path,
            Action<ResourceFieldAttribute, ResourceContext, string> validateCallback = null)
        {
            List<ResourceFieldAttribute> chain = new List<ResourceFieldAttribute>();

            var publicNameParts = path.Split(".");

            foreach (string publicName in publicNameParts[..^1])
            {
                var toOneRelationship = GetToOneRelationship(publicName, resourceContext, path);

                validateCallback?.Invoke(toOneRelationship, resourceContext, path);

                chain.Add(toOneRelationship);
                resourceContext = _resourceContextProvider.GetResourceContext(toOneRelationship.RightType);
            }

            string lastName = publicNameParts[^1];
            var lastField = GetField(lastName, resourceContext, path);

            if (lastField is HasManyAttribute)
            {
                throw new QueryParseException(path == lastName
                    ? $"Field '{lastName}' must be an attribute or a to-one relationship on resource '{resourceContext.PublicName}'."
                    : $"Field '{lastName}' in '{path}' must be an attribute or a to-one relationship on resource '{resourceContext.PublicName}'.");
            }

            validateCallback?.Invoke(lastField, resourceContext, path);

            chain.Add(lastField);
            return chain;
        }

        public RelationshipAttribute GetRelationship(string publicName, ResourceContext resourceContext, string path)
        {
            var relationship = resourceContext.Relationships.FirstOrDefault(r => r.PublicName == publicName);

            if (relationship == null)
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' does not exist on resource '{resourceContext.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' does not exist on resource '{resourceContext.PublicName}'.");
            }

            return relationship;
        }

        public RelationshipAttribute GetToManyRelationship(string publicName, ResourceContext resourceContext, string path)
        {
            var relationship = GetRelationship(publicName, resourceContext, path);

            if (!(relationship is HasManyAttribute))
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' must be a to-many relationship on resource '{resourceContext.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' must be a to-many relationship on resource '{resourceContext.PublicName}'.");
            }

            return relationship;
        }

        public RelationshipAttribute GetToOneRelationship(string publicName, ResourceContext resourceContext, string path)
        {
            var relationship = GetRelationship(publicName, resourceContext, path);

            if (!(relationship is HasOneAttribute))
            {
                throw new QueryParseException(path == publicName
                    ? $"Relationship '{publicName}' must be a to-one relationship on resource '{resourceContext.PublicName}'."
                    : $"Relationship '{publicName}' in '{path}' must be a to-one relationship on resource '{resourceContext.PublicName}'.");
            }

            return relationship;
        }

        private AttrAttribute GetAttribute(string publicName, ResourceContext resourceContext, string path)
        {
            var attribute = resourceContext.Attributes.FirstOrDefault(a => a.PublicName == publicName);

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
            var field = resourceContext.Fields.FirstOrDefault(a => a.PublicName == publicName);

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
