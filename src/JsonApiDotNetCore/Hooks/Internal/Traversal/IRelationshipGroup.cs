using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    internal interface IRelationshipGroup
    {
        RelationshipProxy Proxy { get; }
        HashSet<IIdentifiable> LeftResources { get; }
    }
}