using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Provides metadata for a resource, such as its attributes and relationships.
    /// </summary>
    [PublicAPI]
    public sealed class ResourceContext
    {
        private IReadOnlyCollection<ResourceFieldAttribute> _fields;

        /// <summary>
        /// The publicly exposed resource name.
        /// </summary>
        public string PublicName { get; }

        /// <summary>
        /// The CLR type of the resource.
        /// </summary>
        public Type ResourceType { get; }

        /// <summary>
        /// The identity type of the resource.
        /// </summary>
        public Type IdentityType { get; }

        /// <summary>
        /// Exposed resource attributes. See https://jsonapi.org/format/#document-resource-object-attributes.
        /// </summary>
        public IReadOnlyCollection<AttrAttribute> Attributes { get; }

        /// <summary>
        /// Exposed resource relationships. See https://jsonapi.org/format/#document-resource-object-relationships.
        /// </summary>
        public IReadOnlyCollection<RelationshipAttribute> Relationships { get; }

        /// <summary>
        /// Related entities that are not exposed as resource relationships.
        /// </summary>
        public IReadOnlyCollection<EagerLoadAttribute> EagerLoads { get; }

        /// <summary>
        /// Exposed resource attributes and relationships. See https://jsonapi.org/format/#document-resource-object-fields.
        /// </summary>
        public IReadOnlyCollection<ResourceFieldAttribute> Fields => _fields ??= Attributes.Cast<ResourceFieldAttribute>().Concat(Relationships).ToArray();

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.TopLevelLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.TopLevelLinks" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.TopLevelLinks" /> usage.
        /// </remarks>
        public LinkTypes TopLevelLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.ResourceLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.ResourceLinks" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.ResourceLinks" /> usage.
        /// </remarks>
        public LinkTypes ResourceLinks { get; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks" /> object for all relationships of this resource type.
        /// Defaults to <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.RelationshipLinks" />. This can be overruled per
        /// relationship by setting <see cref="RelationshipAttribute.Links" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.RelationshipLinks" /> usage.
        /// </remarks>
        public LinkTypes RelationshipLinks { get; }

        public ResourceContext(string publicName, Type resourceType, Type identityType, IReadOnlyCollection<AttrAttribute> attributes,
            IReadOnlyCollection<RelationshipAttribute> relationships, IReadOnlyCollection<EagerLoadAttribute> eagerLoads,
            LinkTypes topLevelLinks = LinkTypes.NotConfigured, LinkTypes resourceLinks = LinkTypes.NotConfigured,
            LinkTypes relationshipLinks = LinkTypes.NotConfigured)
        {
            ArgumentGuard.NotNullNorEmpty(publicName, nameof(publicName));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));
            ArgumentGuard.NotNull(identityType, nameof(identityType));
            ArgumentGuard.NotNull(attributes, nameof(attributes));
            ArgumentGuard.NotNull(relationships, nameof(relationships));
            ArgumentGuard.NotNull(eagerLoads, nameof(eagerLoads));

            PublicName = publicName;
            ResourceType = resourceType;
            IdentityType = identityType;
            Attributes = attributes;
            Relationships = relationships;
            EagerLoads = eagerLoads;
            TopLevelLinks = topLevelLinks;
            ResourceLinks = resourceLinks;
            RelationshipLinks = relationshipLinks;
        }

        public override string ToString()
        {
            return PublicName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (ResourceContext)obj;

            return PublicName == other.PublicName && ResourceType == other.ResourceType && IdentityType == other.IdentityType &&
                Attributes.SequenceEqual(other.Attributes) && Relationships.SequenceEqual(other.Relationships) && EagerLoads.SequenceEqual(other.EagerLoads) &&
                TopLevelLinks == other.TopLevelLinks && ResourceLinks == other.ResourceLinks && RelationshipLinks == other.RelationshipLinks;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(PublicName);
            hashCode.Add(ResourceType);
            hashCode.Add(IdentityType);

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
}
