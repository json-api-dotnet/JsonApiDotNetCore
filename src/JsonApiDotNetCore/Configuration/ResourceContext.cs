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
    public class ResourceContext
    {
        private IReadOnlyCollection<ResourceFieldAttribute> _fields;

        /// <summary>
        /// The publicly exposed resource name.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// The CLR type of the resource.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// The identity type of the resource.
        /// </summary>
        public Type IdentityType { get; set; }

        /// <summary>
        /// Exposed resource attributes. See https://jsonapi.org/format/#document-resource-object-attributes.
        /// </summary>
        public IReadOnlyCollection<AttrAttribute> Attributes { get; set; }

        /// <summary>
        /// Exposed resource relationships. See https://jsonapi.org/format/#document-resource-object-relationships.
        /// </summary>
        public IReadOnlyCollection<RelationshipAttribute> Relationships { get; set; }

        /// <summary>
        /// Related entities that are not exposed as resource relationships.
        /// </summary>
        public IReadOnlyCollection<EagerLoadAttribute> EagerLoads { get; set; }

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
        public LinkTypes TopLevelLinks { get; internal set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.ResourceLinks" /> object for this resource type. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.ResourceLinks" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.ResourceLinks" /> usage.
        /// </remarks>
        public LinkTypes ResourceLinks { get; internal set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks" /> object for all relationships of this resource type.
        /// Defaults to <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="IJsonApiOptions.RelationshipLinks" />. This can be overruled per
        /// relationship by setting <see cref="RelationshipAttribute.Links" />.
        /// </summary>
        /// <remarks>
        /// In the process of building the resource graph, this value is set based on <see cref="ResourceLinksAttribute.RelationshipLinks" /> usage.
        /// </remarks>
        public LinkTypes RelationshipLinks { get; internal set; } = LinkTypes.NotConfigured;

        public override string ToString()
        {
            return PublicName;
        }
    }
}
