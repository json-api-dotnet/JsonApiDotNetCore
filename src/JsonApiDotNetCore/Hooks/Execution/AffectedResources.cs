using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using System.Linq;
using System.Collections;
using JsonApiDotNetCore.Internal;
using System;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Basically a enumerable of <typeparamref name="TResource"/> of resources that were affected by the request. 
    /// 
    /// Also contains information about updated relationships through 
    /// implementation of IAffectedRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    public interface IAffectedResources<TResource> : IRelationshipsDictionary<TResource>, IEnumerable<TResource> where TResource : class, IIdentifiable
    {

    }

    /// <summary>
    /// Implementation of IAffectedResources{TResource}.
    /// 
    /// It is basically just a HashSet{TResource} that also stores the 
    /// RelationshipDictionary{TResource} and the same helper methods to access this 
    /// dictionary as defined on IAffectedRelationshipsDictionary{TResource}.
    /// </summary>
    public class AffectedResources<TResource> : HashSet<TResource>, IAffectedResources<TResource> where TResource : class, IIdentifiable
    {
        /// <inheritdoc />
        public RelationshipsDictionary<TResource> AffectedRelationships { get; private set; }

        public AffectedResources(HashSet<TResource> entities,
                        Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(entities)
        {
            AffectedRelationships = new RelationshipsDictionary<TResource>(relationships);
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal AffectedResources(IEnumerable entities,
                        Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this((HashSet<TResource>)entities, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }


        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type principalType)
        {
            return AffectedRelationships.GetByRelationship(principalType);
        }

        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TPrincipalResource>()  where TPrincipalResource : class, IIdentifiable
        {
            return GetByRelationship<TPrincipalResource>();
        }
    }
}