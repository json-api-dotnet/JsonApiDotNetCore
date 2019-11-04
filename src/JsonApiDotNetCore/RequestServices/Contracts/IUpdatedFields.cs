using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Container to register which attributes and relationships are targeted by the current operation.
    /// </summary>
    public interface ITargetedFields
    {
        /// <summary>
        /// List of attributes that are updated by a request
        /// </summary>
        List<AttrAttribute> Attributes { get; set; }
        /// <summary>
        /// List of relationships that are updated by a request
        /// </summary>
        List<RelationshipAttribute> Relationships { get; set; }
    }
}
