using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using System.Collections;
using JsonApiDotNetCore.Internal;
using System;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Basically a enumerable of <typeparamref name="TResource"/> of resources that were affected by the request. 
    /// 
    /// Also contains information about updated relationships through 
    /// implementation of IAffectedRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    public interface IEntityHashSet<TResource> : IByAffectedRelationships<TResource>, IReadOnlyCollection<TResource> where TResource : class, IIdentifiable { }

    /// <summary>
    /// Implementation of IResourceHashSet{TResource}.
    /// 
    /// Basically a enumerable of <typeparamref name="TResource"/> of resources that were affected by the request. 
    /// 
    /// Also contains information about updated relationships through 
    /// implementation of IRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    public class EntityHashSet<TResource> : HashSet<TResource>, IEntityHashSet<TResource> where TResource : class, IIdentifiable
    {
    

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> AffectedRelationships { get => _relationships; }
        private readonly RelationshipsDictionary<TResource> _relationships;

        public EntityHashSet(HashSet<TResource> entities,
                        Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(entities)
        {
            _relationships = new RelationshipsDictionary<TResource>(relationships);
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal EntityHashSet(IEnumerable entities,
                        Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this((HashSet<TResource>)entities, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }


        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type principalType)
        {
            return _relationships.GetByRelationship(principalType);
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRelatedResource>()  where TRelatedResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TRelatedResource));
        }
    }
}