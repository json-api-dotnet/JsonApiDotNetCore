using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using System.Linq;
using System.Collections;

namespace JsonApiDotNetCore.Hooks
{

    public interface IAffectedResources<TEntity> :  IEnumerable<TEntity> where TEntity : class, IIdentifiable 
    {
        HashSet<TEntity> Entities { get; }
    }

    public class AffectedResources<TEntity> : AffectedRelationships<TEntity>, IAffectedResources<TEntity> where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// The entities that are affected by the request.
        /// </summary>
        public HashSet<TEntity> Entities { get; }

        internal AffectedResources(IEnumerable entities,
                                 Dictionary<RelationshipProxy, IEnumerable> relationships) : base(relationships)
        {
            Entities = new HashSet<TEntity>(entities.Cast<TEntity>());
        }
        public IEnumerator<TEntity> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}