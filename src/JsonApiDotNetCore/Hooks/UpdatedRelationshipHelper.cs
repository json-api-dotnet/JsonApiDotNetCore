using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public class HookExecutionContext<TEntity> { }


    public interface IUpdatedRelationshipHelper<TDependent> where TDependent : class, IIdentifiable
    {
        Dictionary<RelationshipAttribute, List<TDependent>> AffectedRelationships();
        Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesRelatedWith<TPrincipal>() where TPrincipal : class, IIdentifiable;
        Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesRelatedWith(Type principalType);
    }

    public class UpdatedRelationshipHelper<TDependent> : IUpdatedRelationshipHelper<TDependent> where TDependent : class, IIdentifiable
    {
        private readonly List<RelationshipGroupEntry> _groups;
        public UpdatedRelationshipHelper(List<RelationshipGroupEntry> relationshipGroups)
        {
            _groups = relationshipGroups;
        }


        public Dictionary<RelationshipAttribute, List<TDependent>> AffectedRelationships()
        {
            return _groups?.ToDictionary(rge => rge.Relationship.Attribute, rge => rge.Entities.Cast<TDependent>().ToList());
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesRelatedWith<TPrincipal>() where TPrincipal : class, IIdentifiable
        {
            return GetEntitiesRelatedWith(typeof(TPrincipal));
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesRelatedWith(Type principalType)
        {
            return _groups?.Where(rge => rge.Relationship.PrincipalType == principalType)
                .ToDictionary(rge => rge.Relationship.Attribute, rge => rge.Entities.Cast<TDependent>().ToList());

        }
    }
}
