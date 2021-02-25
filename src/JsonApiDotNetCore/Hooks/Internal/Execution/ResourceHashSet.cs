using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// Implementation of IResourceHashSet{TResource}.
    /// 
    /// Basically a enumerable of <typeparamref name="TResource"/> of resources that were affected by the request. 
    /// 
    /// Also contains information about updated relationships through 
    /// implementation of IRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    [PublicAPI]
    public class ResourceHashSet<TResource> : HashSet<TResource>, IResourceHashSet<TResource> where TResource : class, IIdentifiable
    {
        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> AffectedRelationships => _relationships;

        private readonly RelationshipsDictionary<TResource> _relationships;

        public ResourceHashSet(HashSet<TResource> resources,
                        Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(resources)
        {
            _relationships = new RelationshipsDictionary<TResource>(relationships);
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal ResourceHashSet(IEnumerable resources,
                        Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this((HashSet<TResource>)resources, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }


        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type resourceType)
        {
            return _relationships.GetByRelationship(resourceType);
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRightResource>() where TRightResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TRightResource));
        }

        /// <inheritdoc />
        public HashSet<TResource> GetAffected(Expression<Func<TResource, object>> navigationAction)
        {
            return _relationships.GetAffected(navigationAction);
        }
    }
}
