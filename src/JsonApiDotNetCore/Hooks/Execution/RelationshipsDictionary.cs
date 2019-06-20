using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    public interface IAffectedRelationships { }

    /// <summary>
    /// An interface that is implemented to expose a relationship dictionary on another class.
    /// </summary>
    public interface IRelationshipsDictionary<TDependentResource> : IRelationshipsDictionaryGetters<TDependentResource> where TDependentResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of affected resources grouped by affected relationships.
        /// </summary>
        RelationshipsDictionary<TDependentResource> AffectedRelationships { get; }
    }

    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which entities.
    /// </summary>
    public interface IRelationshipsDictionaryGetters<TDependentResource> : IAffectedRelationships where TDependentResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <typeparamref name="TPrincipalResource"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable;
        /// <summary>
        /// Gets a dictionary of all entities that have an affected relationship to type <paramref name="principalType"/>
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship(Type principalType);
    }

    /// <summary>
    /// Implementation of IAffectedRelationships{TDependentResource}
    /// 
    /// It is practically a ReadOnlyDictionary{RelationshipAttribute, HashSet{TDependentResource}} dictionary
    /// with the two helper methods defined on IAffectedRelationships{TDependentResource}.
    /// </summary>
    public class RelationshipsDictionary<TDependentResource> : ReadOnlyDictionary<RelationshipAttribute, HashSet<TDependentResource>>, IRelationshipsDictionaryGetters<TDependentResource> where TDependentResource : class, IIdentifiable
    {
        /// <summary>
        /// a dictionary with affected relationships as keys and values being the corresponding resources
        /// that were affected
        /// </summary>
        private readonly Dictionary<RelationshipAttribute, HashSet<TDependentResource>> _groups;

        /// <inheritdoc />
        public RelationshipsDictionary(Dictionary<RelationshipAttribute, HashSet<TDependentResource>> relationships) : base(relationships)
        {
            _groups = relationships;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal RelationshipsDictionary(Dictionary<RelationshipAttribute, IEnumerable> relationships) 
            : this(TypeHelper.ConvertRelationshipDictionary<TDependentResource>(relationships)) { }


        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TPrincipalResource));
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TDependentResource>> GetByRelationship(Type principalType)
        {
            return this.Where(p => p.Key.PrincipalType == principalType).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
