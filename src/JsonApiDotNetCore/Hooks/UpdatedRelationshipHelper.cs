using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
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
        private readonly Dictionary<RelationshipProxy, List<TDependent>> _groups;
        public UpdatedRelationshipHelper(Dictionary<RelationshipProxy, List<IIdentifiable>> entitiesByAffectedRelationship)
        {
            _groups = entitiesByAffectedRelationship.ToDictionary(p => p.Key, p => p.Value.Cast<TDependent>().ToList());
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> AffectedRelationships()
        {
            return _groups.ToDictionary( p => p.Key.Attribute, p => p.Value);
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesRelatedWith<TPrincipal>() where TPrincipal : class, IIdentifiable
        {
            return GetEntitiesRelatedWith(typeof(TPrincipal));
        }

        public Dictionary<RelationshipAttribute, List<TDependent>> GetEntitiesRelatedWith(Type principalType)
        {
            return _groups?.Where( p => p.Key.PrincipalType == principalType).ToDictionary(p => p.Key.Attribute, p => p.Value);
        }
    }
}
