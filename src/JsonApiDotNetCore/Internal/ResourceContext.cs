using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Models.JsonApiDocuments;

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
        /// The field name of the 'id' property
        /// </summary>
        public string IdPropertyName { get; set; }

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

        /// <summary>
        /// Related entities that are not exposed as resource relationships.
        /// </summary>
        public List<EagerLoadAttribute> EagerLoads { get; set; }

        private List<ResourceFieldAttribute> _fields;
        public List<ResourceFieldAttribute> Fields { get { return _fields ??= Attributes.Cast<ResourceFieldAttribute>().Concat(Relationships).ToList();  } }

        /// <summary>
        /// Configures which links to show in the <see cref="TopLevelLinks"/>
        /// object for this resource. If set to <see cref="Links.NotConfigured"/>,
        /// the configuration will be read from <see cref="IJsonApiOptions"/>.
        /// Defaults to <see cref="Links.NotConfigured"/>.
        /// </summary>
        public Links TopLevelLinks { get; internal set; } = Links.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="ResourceLinks"/>
        /// object for this resource. If set to <see cref="Links.NotConfigured"/>,
        /// the configuration will be read from <see cref="IJsonApiOptions"/>.
        /// Defaults to <see cref="Links.NotConfigured"/>.
        /// </summary>
        public Links ResourceLinks { get; internal set; } = Links.NotConfigured;

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// for all relationships of the resource for which this attribute was instantiated.
        /// If set to <see cref="Links.NotConfigured"/>, the configuration will
        /// be read from <see cref="RelationshipAttribute.RelationshipLinks"/>  or
        /// <see cref="IJsonApiOptions"/>. Defaults to <see cref="Links.NotConfigured"/>.
        /// </summary>
        public Links RelationshipLinks { get; internal set; } = Links.NotConfigured;

        public override string ToString()
        {
            return ResourceName;
        }
    }
}
