using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{

    public interface IUpdatedFieldsManager
    {
        List<AttrAttribute> AttributesToUpdate { get; set; }
        List<RelationshipAttribute> RelationshipsToUpdate { get; set; }
    }

    public interface IUpdatedFieldManager_ProposalWithDictionaries
    {
        Dictionary<IIdentifiable, List<AttrAttribute>> AttributesToUpdate { get; set; }
        Dictionary<IIdentifiable, List<RelationshipAttribute>> RelationshipsToUpdate { get; set; }
    }
}
