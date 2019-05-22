
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class that provides insight in what is to be updated. The 
    /// <see cref="IEntityDiff{TEntity}.RequestEntities"/> property reflects what was parsed from the incoming request,
    /// where the <see cref="IEntityDiff{TEntity}.DatabaseEntities"/> reflects what is the current state in the database.
    /// 
    /// Any relationships that are updated can be retrieved via the methods implemented on 
    /// <see cref="IUpdatedRelationshipHelper{TDependent}"/>.
    /// </summary>
    public interface IEntityDiff<TEntity> : IUpdatedRelationshipHelper<TEntity> where TEntity : class, IIdentifiable
    {
        HashSet<TEntity> RequestEntities { get; }
        HashSet<TEntity> DatabaseEntities { get; }
    }

    public class EntityDiff<TEntity> : UpdatedRelationshipHelper<TEntity>, IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        public HashSet<TEntity> RequestEntities { get; private set; }
        public HashSet<TEntity> DatabaseEntities { get; private set; }
        public EntityDiff(IEnumerable requestEntities,
                          IEnumerable databaseEntities,
                          Dictionary<RelationshipProxy, IEnumerable> relationships) : base (relationships)
        {
            RequestEntities = (HashSet<TEntity>)requestEntities;
            DatabaseEntities = (HashSet<TEntity>)databaseEntities;
        }
    }
}
