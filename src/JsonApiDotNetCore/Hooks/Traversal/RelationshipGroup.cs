using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    internal interface IRelationshipGroup
    {
        RelationshipProxy Proxy { get; }
        HashSet<IIdentifiable> LeftResources { get; }
    }

    internal sealed class RelationshipGroup<TRight> : IRelationshipGroup where TRight : class, IIdentifiable
    {
        public RelationshipProxy Proxy { get; }
        public HashSet<IIdentifiable> LeftResources { get; }
        public HashSet<TRight> RightResources { get; internal set; }
        public RelationshipGroup(RelationshipProxy proxy, HashSet<IIdentifiable> leftResources, HashSet<TRight> rightResources)
        {
            Proxy = proxy;
            LeftResources = leftResources;
            RightResources = rightResources;
        }
    }
}
