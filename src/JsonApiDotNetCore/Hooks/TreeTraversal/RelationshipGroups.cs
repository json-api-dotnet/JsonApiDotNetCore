using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// A helper class that, for a given dependent type, stores all affected
    /// entities grouped by the relationship by which these entities are included
    /// in the traversal tree.
    /// </summary>
    public class RelationshipGroups
    {
        private readonly Dictionary<string, RelationshipGroupEntry> _entitiesByRelationship;
        public RelationshipGroups()
        {
            _entitiesByRelationship = new Dictionary<string, RelationshipGroupEntry>();
        }

        /// <summary>
        /// Add the specified proxy and relatedEntities.
        /// </summary>
        /// <param name="proxy">Proxy.</param>
        /// <param name="relatedEntities">Related entities.</param>
        public void Add(RelationshipProxy proxy, IEnumerable<IIdentifiable> relatedEntities)
        {
            var key = proxy.RelationshipIdentifier;
            if (!_entitiesByRelationship.TryGetValue(key, out var entitiesWithRelationship))
            {
                entitiesWithRelationship = new RelationshipGroupEntry(proxy, new HashSet<IIdentifiable>(relatedEntities));
                _entitiesByRelationship[key] = entitiesWithRelationship;
            }
            else
            {
                entitiesWithRelationship.Entities.Union(new HashSet<IIdentifiable>(relatedEntities));
            }
        }

        /// <summary>
        /// Entries in this instance.
        /// </summary>
        /// <returns>The entries.</returns>
        public List<RelationshipGroupEntry> Entries()
        {
            return _entitiesByRelationship.Select(kvPair => kvPair.Value).ToList();
        }

    }
}

