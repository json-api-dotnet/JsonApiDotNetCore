using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Internal
{
    public class ResourceContext
    {
        /// <summary>
        /// The exposed resource name
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// The data model type
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// The identity member type
        /// </summary>
        public Type IdentityType { get; set; }

        /// <summary>
        /// The concrete <see cref="ResourceDefinition{T}"/> type.
        /// We store this so that we don't need to re-compute the generic type.
        /// </summary>
        public Type ResourceDefinitionType { get; set; }

        /// <summary>
        /// Exposed resource attributes.
        /// See https://jsonapi.org/format/#document-resource-object-attributes.
        /// </summary>
        public List<AttrAttribute> Attributes { get; set; }

        /// <summary>
        /// Exposed resource relationships.
        /// See https://jsonapi.org/format/#document-resource-object-relationships
        /// </summary>
        public List<RelationshipAttribute> Relationships { get; set; }

        private List<IResourceField> _fields;
        public List<IResourceField> Fields { get { return _fields = _fields ?? Attributes.Cast<IResourceField>().Concat(Relationships).ToList();  } }

        /// <summary>
        /// Configures which links to show in the <see cref="TopLevelLinks"/>
        /// object for this resource. If set to <see cref="Link.NotConfigured"/>,
        /// the configuration will be read from <see cref="ILinksConfiguration"/>.
        ///  Defaults to <see cref="Link.NotConfigured"/>.
        /// </summary>
        public Link TopLevelLinks { get; internal set; } = Link.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="ResourceLinks"/>
        /// object for this resource. If set to <see cref="Link.NotConfigured"/>,
        /// the configuration will be read from <see cref="ILinksConfiguration"/>.
        /// Defaults to <see cref="Link.NotConfigured"/>.
        /// </summary>
        public Link ResourceLinks { get; internal set; } = Link.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// for all relationships of the resource for which this attribute was instantiated.
        /// If set to <see cref="Link.NotConfigured"/>, the configuration will
        /// be read from <see cref="RelationshipAttribute.RelationshipLinks"/>  or
        /// <see cref="ILinksConfiguration"/>. Defaults to <see cref="Link.NotConfigured"/>.
        /// </summary>
        public Link RelationshipLinks { get; internal set; } = Link.NotConfigured;

    }
}
