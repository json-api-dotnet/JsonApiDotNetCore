using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using System.Linq;
using System.Collections;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// Basically just a list of <typeparamref name="TResource"/>, but also contains information
    /// about updated relationships through inheritance of IAffectedRelationships<typeparamref name="TResource"/>>
    /// </summary>
    public interface IAffectedResources<TResource> : IAffectedRelationships<TResource>, IEnumerable<TResource> where TResource : class, IIdentifiable 
    {
        /// <summary>
        /// The entities that are affected by the request.
        /// </summary>
        HashSet<TResource> Resources { get; }
    }

    public class AffectedResources<TResource> : AffectedRelationships<TResource>, IAffectedResources<TResource> where TResource : class, IIdentifiable
    {
        /// <inheritdoc />
        public HashSet<TResource> Resources { get; }

        public AffectedResources(HashSet<TResource> entities,
                        Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(relationships)
        {
            Resources = new HashSet<TResource>(entities.Cast<TResource>());
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal AffectedResources(IEnumerable entities,
                        Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this((HashSet<TResource>)entities, ConvertRelationshipDictionary(relationships)) { }

        /// <inheritdoc />
        public IEnumerator<TResource> GetEnumerator()
        {
            return Resources.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}