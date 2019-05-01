using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IHookExecutionContext<TDependent> where TDependent : class, IIdentifiable
    {
        ResourceAction Pipeline { get; }
        Dictionary<RelationshipAttribute, List<TDependent>> GetAllAffectedRelationships();
        Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesForAffectedRelationship<TPrincipal>() where TPrincipal : class, IIdentifiable;
        Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesForAffectedRelationship(Type principalType);
    }

    public class HookExecutionContext<TDependent> : IHookExecutionContext<TDependent> where TDependent : class, IIdentifiable
    {
        public ResourceAction Pipeline { get; private set; }
        private readonly List<RelationshipGroupEntry> _groups;
        public HookExecutionContext(ResourceAction pipeline, List<RelationshipGroupEntry> relationshipGroups = null)
        {
            Pipeline = pipeline;
            _groups = relationshipGroups;
        }


        public Dictionary<RelationshipAttribute, List<TDependent>> GetAllAffectedRelationships()
        {
            return _groups?.ToDictionary(rge => rge.Relationship.Attribute, rge => rge.Entities.Cast<TDependent>().ToList());
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesForAffectedRelationship<TPrincipal>() where TPrincipal : class, IIdentifiable
        {
            return GetEntitiesForAffectedRelationship(typeof(TPrincipal));
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesForAffectedRelationship(Type principalType)
        {
            return _groups?.Where(rge => rge.Relationship.PrincipalType == principalType)
                .ToDictionary(rge => rge.Relationship.Attribute, rge => rge.Entities.Cast<TDependent>().ToList());

        }
    }
}
