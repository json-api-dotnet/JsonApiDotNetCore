using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using DependentType = System.Type;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Related entities in current layer entry.
    /// </summary>
    public class NodeInLayer
    {
        private readonly HashSet<IIdentifiable> _uniqueSet;

        public DependentType DependentType { get; private set; }
        public List<RelationshipGroupEntry> RelationshipGroups { get; private set; }
        public IList UniqueSet { get { return TypeHelper.ConvertCollection(_uniqueSet, DependentType); } }

        public NodeInLayer(
            DependentType dependentType,
            HashSet<IIdentifiable> uniqueSet,
            List<RelationshipGroupEntry> relationshipGroups
        )
        {
            _uniqueSet = uniqueSet;
            DependentType = dependentType;
            RelationshipGroups = relationshipGroups;
        }

        public void UpdateUniqueSet(IEnumerable filteredUniqueSet)
        {
            var casted = new HashSet<IIdentifiable>(filteredUniqueSet.Cast<IIdentifiable>());
            _uniqueSet.IntersectWith(casted);
        }
    }
}

