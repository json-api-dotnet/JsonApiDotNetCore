using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal
{
    public class ContextEntity
    {
        /// <summary>
        /// The exposed resource name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// The data model type
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// The identity member type
        /// </summary>
        public Type IdentityType { get; set; }

        /// <summary>
        /// The concrete <see cref="ResourceDefinition{T}"/> type.
        /// We store this so that we don't need to re-compute the generic type.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Exposed resource attributes
        /// </summary>
        public List<AttrAttribute> Attributes { get; set; }

        /// <summary>
        /// Exposed resource relationships
        /// </summary>
        public List<RelationshipAttribute> Relationships { get; set; }

        /// <summary>
        /// Links to include in resource responses
        /// </summary>
        public Link Links { get; set; } = Link.All;
    }
}
