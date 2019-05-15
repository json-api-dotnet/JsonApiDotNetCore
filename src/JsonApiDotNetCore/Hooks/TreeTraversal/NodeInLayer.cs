using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;


namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Related entities in current layer entry.
    /// </summary>
    public class NodeInLayer
    {
        private HashSet<IIdentifiable> _uniqueSet;

        public Dictionary<RelationshipProxy, List<IIdentifiable>> EntitiesByRelationships { get; private set; }
        public Dictionary<RelationshipProxy, List<IIdentifiable>> PrincipalEntitiesByRelationships { get; private set; }
        public List<RelationshipProxy> Relationships { get; private set; }
        public IList UniqueSet { get { return TypeHelper.ConvertCollection(_uniqueSet, EntityType); } }
        public Type EntityType { get; internal set; }

        public NodeInLayer(
            Type principalType,
            HashSet<IIdentifiable> uniqueSet,
            Dictionary<RelationshipProxy, List<IIdentifiable>> entitiesByRelationships,
            Dictionary<RelationshipProxy, List<IIdentifiable>> principalEntitiesByRelationships,
            List<RelationshipProxy> relationships
        )
        {
            _uniqueSet = uniqueSet;
            EntityType = principalType;
            EntitiesByRelationships = entitiesByRelationships;
            PrincipalEntitiesByRelationships = principalEntitiesByRelationships;
            Relationships = relationships;
        }



        public void UpdateUniqueSet(IEnumerable filteredUniqueSet)
        {
            var casted = filteredUniqueSet.Cast<IIdentifiable>().ToList();
            _uniqueSet = new HashSet<IIdentifiable>(_uniqueSet.Intersect(casted, ResourceHookExecutor.Comparer));

            EntitiesByRelationships = EntitiesByRelationships.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Intersect(casted, ResourceHookExecutor.Comparer).ToList());

        }
    }
}

