using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public interface IUpdatedFields
    {
        List<AttrAttribute> Attributes { get; set; }
        List<RelationshipAttribute> Relationships { get; set; }
    }

}
