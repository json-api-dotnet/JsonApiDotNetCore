using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    public interface IAffectedRelationships { }

    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which entities.
    /// </summary>
    public interface IAffectedRelationships<TDependent> : IAffectedRelationships where TDependent : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of all entities grouped by affected relationship.
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependent>> AllByRelationships();

        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <typeparamref name="TPrincipal"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependent>> GetByRelationship<TPrincipal>() where TPrincipal : class, IIdentifiable;
        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <paramref name="principalType"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependent>> GetByRelationship(Type principalType);
    }

    public class AffectedRelationships<TDependent> : IAffectedRelationships<TDependent> where TDependent : class, IIdentifiable
    {
        private readonly Dictionary<RelationshipProxy, HashSet<TDependent>> _groups;

        public Dictionary<RelationshipAttribute, HashSet<TDependent>> AllByRelationships()
        {
            return _groups?.ToDictionary(p => p.Key.Attribute, p => p.Value);
        }
        public AffectedRelationships(Dictionary<RelationshipProxy, IEnumerable> relationships)
        {
            _groups = relationships.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TDependent>((IEnumerable<TDependent>)kvp.Value));
        }

        public Dictionary<RelationshipAttribute, HashSet<TDependent>> GetByRelationship<TPrincipal>() where TPrincipal : class, IIdentifiable
        {
            return GetByRelationship(typeof(TPrincipal));
        }

        public Dictionary<RelationshipAttribute, HashSet<TDependent>> GetByRelationship(Type principalType)
        {
            return _groups?.Where(p => p.Key.PrincipalType == principalType).ToDictionary(p => p.Key.Attribute, p => p.Value);
        }
    }
}
