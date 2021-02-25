using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    internal sealed class RelationshipGroup<TRight> : IRelationshipGroup
        where TRight : class, IIdentifiable
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
