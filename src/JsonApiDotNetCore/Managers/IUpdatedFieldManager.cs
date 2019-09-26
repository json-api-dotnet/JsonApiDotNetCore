using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public interface IUpdatedFields
    {
        List<AttrAttribute> AttributesToUpdate { get; set; }
        List<RelationshipAttribute> RelationshipsToUpdate { get; set; }
    }

    public class UpdatedFields: IUpdatedFields
    {
        public List<AttrAttribute> AttributesToUpdate { get; set; } = new List<AttrAttribute>();
        public List<RelationshipAttribute> RelationshipsToUpdate { get; set; } = new List<RelationshipAttribute>();
    }

}
