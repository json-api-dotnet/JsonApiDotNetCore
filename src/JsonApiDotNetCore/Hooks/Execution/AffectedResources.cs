using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using System.Linq;
using System.Collections;

namespace JsonApiDotNetCore.Hooks
{

    public interface IAffectedResources<TEntity> : IAffectedResourcesBase<TEntity>, IEnumerable<TEntity> where TEntity : class, IIdentifiable 
    {

    }

    /// NOTE: you might wonder why there is a separate AffectedResourceBase and AffectedResource.
    /// If we merge them together, ie get rid of the base and just let the AffectedResource directly implement IEnumerable{TEntity},
    /// we will run in to problems with the following:
    /// EntityDiff{<typeparam name="TEntity"/>} inherits from AffectedResource{TEntity},
    /// but EntityDiff also implements IEnumerable{EntityDiffPair{TEntity}}. This means that
    /// EntityDiff will implement two IEnumerable{x} where (x1 = TEntity and x2 = EntityDiffPair{TEntity} )
    /// The problem with this is that when you then try to do a simple foreach loop over
    /// a EntityDiff instance, it will throw an error, because it does not know which of the two enumerators to pick.
    /// We want EntityDiff to only loop over the EntityDiffPair, so we can do that by making sure
    /// it doesn't inherit the IEnumerable{TEntity} part from AffectedResources.
    public interface IAffectedResourcesBase<TEntity>  where TEntity : class, IIdentifiable
    {
        HashSet<TEntity> Entities { get; }
    }

    public class AffectedResources<TEntity> : AffectedResourcesBase<TEntity>, IAffectedResources<TEntity> where TEntity : class, IIdentifiable
    {
        internal AffectedResources(IEnumerable entities,
                                   Dictionary<RelationshipProxy, IEnumerable> relationships) 
                                   : base(entities, relationships) { }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class AffectedResourcesBase<TEntity> : AffectedRelationships<TEntity>, IAffectedResourcesBase<TEntity> where TEntity : class, IIdentifiable
    {
        public HashSet<TEntity> Entities { get; }

        internal protected AffectedResourcesBase(IEnumerable entities,
                                 Dictionary<RelationshipProxy, IEnumerable> relationships) : base(relationships)
        {
            Entities = new HashSet<TEntity>(entities.Cast<TEntity>());
        }
    }

}