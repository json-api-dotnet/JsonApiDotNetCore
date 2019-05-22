using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
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
