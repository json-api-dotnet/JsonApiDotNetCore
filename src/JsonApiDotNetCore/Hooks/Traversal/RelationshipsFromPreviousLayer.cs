using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class for mapping relationships between a current and previous layer
    /// </summary>
   internal interface IRelationshipsFromPreviousLayer
    {
        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the entities of the current layer
        /// </summary>
        /// <returns>The right side entities.</returns>
        Dictionary<RelationshipAttribute, IEnumerable> GetRightEntities();
        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the entities of the previous layer
        /// </summary>
        /// <returns>The right side entities.</returns>
        Dictionary<RelationshipAttribute, IEnumerable> GetLeftEntities();
    }

    internal class RelationshipsFromPreviousLayer<TRightResource> : IRelationshipsFromPreviousLayer, IEnumerable<RelationshipGroup<TRightResource>> where TRightResource : class, IIdentifiable
    {
        readonly IEnumerable<RelationshipGroup<TRightResource>> _collection;

        public RelationshipsFromPreviousLayer(IEnumerable<RelationshipGroup<TRightResource>> collection)
        {
            _collection = collection;
        }

        /// <inheritdoc/>
        public Dictionary<RelationshipAttribute, IEnumerable> GetRightEntities()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.RightEntities);
        }

        /// <inheritdoc/>
        public Dictionary<RelationshipAttribute, IEnumerable> GetLeftEntities()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.LeftEntities);
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
