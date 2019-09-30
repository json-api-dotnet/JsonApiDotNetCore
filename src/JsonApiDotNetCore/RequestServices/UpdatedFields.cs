using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public class UpdatedFields : IUpdatedFields
    {
        /// <inheritdoc/>
        public List<AttrAttribute> Attributes { get; set; } = new List<AttrAttribute>();
        /// <inheritdoc/>
        public List<RelationshipAttribute> Relationships { get; set; } = new List<RelationshipAttribute>();
    }

}
