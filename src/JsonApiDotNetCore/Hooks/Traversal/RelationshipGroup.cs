using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    internal interface IRelationshipGroup
    {
        RelationshipProxy Proxy { get; }
        HashSet<IIdentifiable> LeftEntities { get; }
    }

    internal sealed class RelationshipGroup<TRight> : IRelationshipGroup where TRight : class, IIdentifiable
    {
        public RelationshipProxy Proxy { get; }
        public HashSet<IIdentifiable> LeftEntities { get; }
        public HashSet<TRight> RightEntities { get; internal set; }
        public RelationshipGroup(RelationshipProxy proxy, HashSet<IIdentifiable> leftEntities, HashSet<TRight> rightEntities)
        {
            Proxy = proxy;
            LeftEntities = leftEntities;
            RightEntities = rightEntities;
        }
    }
}
