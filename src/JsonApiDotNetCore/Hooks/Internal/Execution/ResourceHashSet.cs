using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// Implementation of IResourceHashSet{TResource}. Basically a enumerable of <typeparamref name="TResource" /> of resources that were affected by the
    /// request. Also contains information about updated relationships through implementation of IRelationshipsDictionary<typeparamref name="TResource" />>
    /// </summary>
    [PublicAPI]
    public class ResourceHashSet<TResource> : HashSet<TResource>, IResourceHashSet<TResource>
        where TResource : class, IIdentifiable
    {
        private readonly RelationshipsDictionary<TResource> _relationships;

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, HashSet<TResource>> AffectedRelationships => _relationships;

        public ResourceHashSet(HashSet<TResource> resources, IDictionary<RelationshipAttribute, HashSet<TResource>> relationships)
            : base(resources)
        {
            _relationships = new RelationshipsDictionary<TResource>(relationships);
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal ResourceHashSet(IEnumerable resources, IDictionary<RelationshipAttribute, IEnumerable> relationships)
            : this((HashSet<TResource>)resources, relationships.ToDictionary(pair => pair.Key, pair => (HashSet<TResource>)pair.Value))
        {
        }

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type resourceType)
        {
            return _relationships.GetByRelationship(resourceType);
        }

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TRightResource>()
            where TRightResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TRightResource));
        }

        /// <inheritdoc />
        public virtual HashSet<TResource> GetAffected(Expression<Func<TResource, object>> navigationAction)
        {
            return _relationships.GetAffected(navigationAction);
        }
    }
}
