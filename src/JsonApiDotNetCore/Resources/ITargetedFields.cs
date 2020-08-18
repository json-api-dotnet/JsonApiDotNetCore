using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Container to register which attributes and relationships are targeted by the current operation.
    /// </summary>
    public interface ITargetedFields
    {
        /// <summary>
        /// List of attributes that are targeted by a request
        /// </summary>
        IList<AttrAttribute> Attributes { get; set; }

        /// <summary>
        /// List of relationships that are targeted by a request
        /// </summary>
        IList<RelationshipAttribute> Relationships { get; set; }
    }
}
