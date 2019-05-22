using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    internal interface IRelationshipGroup
    {
        RelationshipProxy Proxy { get; }
        HashSet<IIdentifiable> PrincipalEntities { get; }
    }

    internal class RelationshipGroup<TDependent> : IRelationshipGroup where TDependent : class, IIdentifiable
    {
        public RelationshipProxy Proxy { get; }
        public HashSet<IIdentifiable> PrincipalEntities { get; }
        public HashSet<TDependent> DependentEntities { get; internal set; }
        public RelationshipGroup(RelationshipProxy proxy, HashSet<IIdentifiable> principalEntities, HashSet<TDependent> dependentEntities)
        {
            Proxy = proxy;
            PrincipalEntities = principalEntities;
            DependentEntities = dependentEntities;
        }
    }
}