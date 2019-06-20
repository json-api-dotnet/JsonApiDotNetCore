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
        /// <returns>The dependent entities.</returns>
        Dictionary<RelationshipAttribute, IEnumerable> GetDependentEntities();
        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the entities of the previous layer
        /// </summary>
        /// <returns>The dependent entities.</returns>
        Dictionary<RelationshipAttribute, IEnumerable> GetPrincipalEntities();
    }

    internal class RelationshipsFromPreviousLayer<TDependent> : IRelationshipsFromPreviousLayer, IEnumerable<RelationshipGroup<TDependent>> where TDependent : class, IIdentifiable
    {
        readonly IEnumerable<RelationshipGroup<TDependent>> _collection;

        public RelationshipsFromPreviousLayer(IEnumerable<RelationshipGroup<TDependent>> collection)
        {
            _collection = collection;
        }

        public Dictionary<RelationshipAttribute, IEnumerable> GetDependentEntities()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.DependentEntities);
        }

        public Dictionary<RelationshipAttribute, IEnumerable> GetPrincipalEntities()
        {
            return _collection.ToDictionary(rg => rg.Proxy.Attribute, rg => (IEnumerable)rg.PrincipalEntities);
        }

        public IEnumerator<RelationshipGroup<TDependent>> GetEnumerator()
        {
            return _collection.Cast<RelationshipGroup<TDependent>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
