using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
   internal interface IRelationshipsFromPreviousLayer
    {
        Dictionary<RelationshipProxy, IEnumerable> GetDependentEntities();
        Dictionary<RelationshipProxy, IEnumerable> GetPrincipalEntities();
    }

    internal class RelationshipsFromPreviousLayer<TDependent> : IRelationshipsFromPreviousLayer, IEnumerable<RelationshipGroup<TDependent>> where TDependent : class, IIdentifiable
    {
        readonly IEnumerable<RelationshipGroup<TDependent>> _collection;

        public RelationshipsFromPreviousLayer(IEnumerable<RelationshipGroup<TDependent>> collection)
        {
            _collection = collection;
        }

        public Dictionary<RelationshipProxy, IEnumerable> GetDependentEntities()
        {
            return _collection.ToDictionary(rg => rg.Proxy, rg => (IEnumerable)rg.DependentEntities);
        }

        public Dictionary<RelationshipProxy, IEnumerable> GetPrincipalEntities()
        {
            return _collection.ToDictionary(rg => rg.Proxy, rg => (IEnumerable)rg.PrincipalEntities);
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
