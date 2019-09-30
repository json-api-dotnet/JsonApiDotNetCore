using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public interface IUpdatedFields
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
