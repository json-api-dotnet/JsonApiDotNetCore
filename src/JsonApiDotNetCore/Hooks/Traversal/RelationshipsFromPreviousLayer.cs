using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class for mapping relationships between a current and previous layer
    /// </summary>
   internal interface IRelationshipsFromPreviousLayer
    {
        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the resources of the current layer
        /// </summary>
        /// <returns>The right side resources.</returns>
        Dictionary<RelationshipAttribute, IEnumerable> GetRightResources();
        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the resources of the previous layer
        /// </summary>
        /// <returns>The right side resources.</returns>
        Dictionary<RelationshipAttribute, IEnumerable> GetLeftResources();
    }

    internal sealed class RelationshipsFromPreviousLayer<TRightResource> : IRelationshipsFromPreviousLayer, IEnumerable<RelationshipGroup<TRightResource>> where TRightResource : class, IIdentifiable
    {
        private readonly IEnumerable<RelationshipGroup<TRightResource>> _collection;

        public RelationshipsFromPreviousLayer(IEnumerable<RelationshipGroup<TRightResource>> collection)
        {
            _collection = collection;
        }

        /// <inheritdoc/>
        public Dictionary<RelationshipAttribute, IEnumerable> GetRightResources()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.RightResources);
        }

        /// <inheritdoc/>
        public Dictionary<RelationshipAttribute, IEnumerable> GetLeftResources()
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
