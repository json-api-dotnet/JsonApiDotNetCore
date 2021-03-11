using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    internal sealed class RelationshipsFromPreviousLayer<TRightResource> : IRelationshipsFromPreviousLayer, IEnumerable<RelationshipGroup<TRightResource>>
        where TRightResource : class, IIdentifiable
    {
        private readonly IEnumerable<RelationshipGroup<TRightResource>> _collection;

        public RelationshipsFromPreviousLayer(IEnumerable<RelationshipGroup<TRightResource>> collection)
        {
            _collection = collection;
        }

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, IEnumerable> GetRightResources()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.RightResources);
        }

        /// <inheritdoc />
        public IDictionary<RelationshipAttribute, IEnumerable> GetLeftResources()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.LeftResources);
        }

        public IEnumerator<RelationshipGroup<TRightResource>> GetEnumerator()
        {
            return _collection.Cast<RelationshipGroup<TRightResource>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
