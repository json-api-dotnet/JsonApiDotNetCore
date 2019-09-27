using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public interface IUpdatedFields
    {
        List<AttrAttribute> Attributes { get; set; }
        List<RelationshipAttribute> Relationships { get; set; }
    }

    public class UpdatedFields: IUpdatedFields
    {
        public List<AttrAttribute> Attributes { get; set; } = new List<AttrAttribute>();
        public List<RelationshipAttribute> Relationships { get; set; } = new List<RelationshipAttribute>();
    }

}
