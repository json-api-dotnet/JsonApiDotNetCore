using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc/>
    public sealed class TargetedFields : ITargetedFields
    {
        /// <inheritdoc/>
        public List<AttrAttribute> Attributes { get; set; } = new List<AttrAttribute>();
        /// <inheritdoc/>
        public List<RelationshipAttribute> Relationships { get; set; } = new List<RelationshipAttribute>();
    }

}
