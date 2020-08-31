using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    public sealed class TargetedFields : ITargetedFields
    {
        /// <inheritdoc />
        public IList<AttrAttribute> Attributes { get; set; } = new List<AttrAttribute>();

        /// <inheritdoc />
        public IList<RelationshipAttribute> Relationships { get; set; } = new List<RelationshipAttribute>();
    }
}
