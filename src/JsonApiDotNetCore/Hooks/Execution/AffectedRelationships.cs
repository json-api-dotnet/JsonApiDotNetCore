using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    public interface IAffectedRelationships { }

    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which entities.
    /// </summary>
    public interface IAffectedRelationships<TDependentResource> : IAffectedRelationships where TDependentResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of all entities grouped by affected relationship.
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependentResource>> AllByRelationships();

        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <typeparamref name="TPrincipalResource"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable;
        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <paramref name="principalType"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship(Type principalType);
    }

    /// <inheritdoc />
    public class AffectedRelationships<TDependentResource> :  IAffectedRelationships<TDependentResource> where TDependentResource : class, IIdentifiable
    {
        internal static Dictionary<RelationshipAttribute, HashSet<TDependentResource>> ConvertRelationshipDictionary(Dictionary<RelationshipAttribute, IEnumerable> relationships)
        {
            return relationships.ToDictionary(pair => pair.Key, pair => (HashSet<TDependentResource>)pair.Value);
        }

        /// <summary>
        /// a dictionary with affected relationships as keys and values being the corresponding resources
        /// that were affected
        /// </summary>
        private readonly Dictionary<RelationshipAttribute, HashSet<TDependentResource>> _groups;

        /// <inheritdoc />
        public AffectedRelationships(Dictionary<RelationshipAttribute, HashSet<TDependentResource>> relationships)
        {
            _groups = relationships;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal AffectedRelationships(Dictionary<RelationshipAttribute, IEnumerable> relationships) : this(ConvertRelationshipDictionary(relationships)) { }

        public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> AllByRelationships()
        {
            return _groups;
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TPrincipalResource));
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship(Type principalType)
        {
            return _groups?.Where(p => p.Key.PrincipalType == principalType).ToDictionary(p => p.Key, p => p.Value);
        }
    }

    ///// <inheritdoc />
    //public class AffectedRelationships<TDependentResource> : ReadOnlyDictionary<RelationshipAttribute, HashSet<TDependentResource>>, IAffectedRelationships<TDependentResource> where TDependentResource : class, IIdentifiable
    //{
    //    private readonly Dictionary<RelationshipAttribute, HashSet<TDependentResource>> _groups;

    //    private static IDictionary<RelationshipAttribute, HashSet<TDependentResource>> test(Dictionary<RelationshipAttribute, IEnumerable> relationship)
    //    {
    //        return relationship.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TDependentResource>((IEnumerable<TDependentResource>)kvp.Value));
    //    }

    //    public AffectedRelationships(Dictionary<RelationshipAttribute, IEnumerable> relationship) : base(test(relationship))
    //    {
    //    }


    //    /// <inheritdoc />
    //    public AffectedRelationships(Dictionary<RelationshipAttribute, IEnumerable> relationships)
    //    {
    //        _groups = relationships.ToDictionary(kvp => kvp.Key, kvp => new HashSet<TDependentResource>((IEnumerable<TDependentResource>)kvp.Value));
    //    }

    //    public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> AllByRelationships()
    //    {
    //        return _groups;
    //    }



    //    /// <inheritdoc />
    //    public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable
    //    {
    //        return GetByRelationship(typeof(TPrincipalResource));
    //    }

    //    /// <inheritdoc />
    //    public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship(Type principalType)
    //    {
    //        return _groups?.Where(p => p.Key.PrincipalType == principalType).ToDictionary(p => p.Key, p => p.Value);
    //    }
    //}
}
