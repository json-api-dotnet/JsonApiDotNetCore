using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Container to register which resource attributes and relationships are targeted by a request.
    /// </summary>
    public interface ITargetedFields
    {
        /// <summary>
        /// The set of attributes that are targeted by a request.
        /// </summary>
        ISet<AttrAttribute> Attributes { get; set; }

        /// <summary>
        /// The set of relationships that are targeted by a request.
        /// </summary>
        ISet<RelationshipAttribute> Relationships { get; set; }
    }
}
