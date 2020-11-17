using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    public sealed class TargetedFields : ITargetedFields
    {
        /// <inheritdoc />
        public ISet<AttrAttribute> Attributes { get; set; } = new HashSet<AttrAttribute>();

        /// <inheritdoc />
        public ISet<RelationshipAttribute> Relationships { get; set; } = new HashSet<RelationshipAttribute>();
    }
}
