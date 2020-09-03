using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Provides metadata for a resource, such as its attributes and relationships.
    /// </summary>
    public class ResourceContext
    {
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
        /// The concrete <see cref="ResourceDefinition{TResource}"/> type.
        /// We store this so that we don't need to re-compute the generic type.
        /// </summary>
        public Type ResourceDefinitionType { get; set; }

        /// <summary>
        /// Exposed resource attributes.
        /// See https://jsonapi.org/format/#document-resource-object-attributes.
        /// </summary>
        public IReadOnlyCollection<AttrAttribute> Attributes { get; set; }

        /// <summary>
        /// Exposed resource relationships.
        /// See https://jsonapi.org/format/#document-resource-object-relationships.
        /// </summary>
        public IReadOnlyCollection<RelationshipAttribute> Relationships { get; set; }

        /// <summary>
        /// Related entities that are not exposed as resource relationships.
        /// </summary>
        public IReadOnlyCollection<EagerLoadAttribute> EagerLoads { get; set; }

        private IReadOnlyCollection<ResourceFieldAttribute> _fields;
        
        /// <summary>
        /// Exposed resource attributes and relationships.
        /// See https://jsonapi.org/format/#document-resource-object-fields.
        /// </summary>
        public IReadOnlyCollection<ResourceFieldAttribute> Fields => _fields ??= Attributes.Cast<ResourceFieldAttribute>().Concat(Relationships).ToArray();

        /// <summary>
        /// Configures which links to show in the <see cref="TopLevelLinks"/>
        /// object for this resource. If set to <see cref="LinkTypes.NotConfigured"/>,
        /// the configuration will be read from <see cref="IJsonApiOptions"/>.
        /// Defaults to <see cref="LinkTypes.NotConfigured"/>.
        /// </summary>
        public LinkTypes TopLevelLinks { get; internal set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="ResourceLinks"/>
        /// object for this resource. If set to <see cref="LinkTypes.NotConfigured"/>,
        /// the configuration will be read from <see cref="IJsonApiOptions"/>.
        /// Defaults to <see cref="LinkTypes.NotConfigured"/>.
        /// </summary>
        public LinkTypes ResourceLinks { get; internal set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// for all relationships of the resource for which this attribute was instantiated.
        /// If set to <see cref="LinkTypes.NotConfigured"/>, the configuration will
        /// be read from <see cref="RelationshipAttribute.Links"/>  or
        /// <see cref="IJsonApiOptions"/>. Defaults to <see cref="LinkTypes.NotConfigured"/>.
        /// </summary>
        public LinkTypes RelationshipLinks { get; internal set; } = LinkTypes.NotConfigured;

        public override string ToString()
        {
            return PublicName;
        }
    }
}
