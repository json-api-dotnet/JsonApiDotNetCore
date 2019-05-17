using System.Collections;
using JsonApiDotNetCore.Internal;
using DependentType = System.Type;

namespace JsonApiDotNetCore.Services
{
    internal interface IEntityNode
    {
        DependentType EntityType { get; }
        IEnumerable UniqueEntities { get; }
        RelationshipProxy[] RelationshipsToNextLayer { get; }
        IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer { get; }

        void Reassign(IEnumerable source = null);
        void UpdateUnique(IEnumerable updated);
    }

}
