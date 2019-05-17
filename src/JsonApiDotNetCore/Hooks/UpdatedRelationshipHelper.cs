using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IUpdatedRelationshipHelper { }

    public interface IUpdatedRelationshipHelper<TDependent> : IUpdatedRelationshipHelper where TDependent : class, IIdentifiable
    {
        Dictionary<RelationshipAttribute, HashSet<TDependent>> AllEntitiesByRelation { get; }
        Dictionary<RelationshipAttribute, HashSet<TDependent>> EntitiesRelatedTo<TPrincipal>() where TPrincipal : class, IIdentifiable;
        Dictionary<RelationshipAttribute, HashSet<TDependent>> EntitiesRelatedTo(Type principalType);
    }

    public class UpdatedRelationshipHelper<TDependent> : IUpdatedRelationshipHelper<TDependent> where TDependent : class, IIdentifiable
    {
        private readonly Dictionary<RelationshipProxy, HashSet<TDependent>> _groups;
        public Dictionary<RelationshipAttribute, HashSet<TDependent>> ImplicitUpdates { get; }

        public Dictionary<RelationshipAttribute, HashSet<TDependent>> AllEntitiesByRelation
        {
            get { return _groups?.ToDictionary(p => p.Key.Attribute, p => p.Value); }
        }
        public UpdatedRelationshipHelper(Dictionary<RelationshipProxy, IEnumerable> prevLayerRelationships)
        {
            _groups = prevLayerRelationships.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TDependent>((IEnumerable<TDependent>)kvp.Value));
        }

        public Dictionary<RelationshipAttribute, HashSet<TDependent>> EntitiesRelatedTo<TPrincipal>() where TPrincipal : class, IIdentifiable
        {
            return EntitiesRelatedTo(typeof(TPrincipal));
        }

        public Dictionary<RelationshipAttribute, HashSet<TDependent>> EntitiesRelatedTo(Type principalType)
        {
            return _groups?.Where(p => p.Key.PrincipalType == principalType).ToDictionary(p => p.Key.Attribute, p => p.Value);
        }
    }
}
