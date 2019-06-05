
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{

    /// <summary>
    /// A helper class that provides insight in what is to be updated. The 
    /// <see cref="IEntityDiff{TEntity}.RequestEntities"/> property reflects what was parsed from the incoming request,
    /// where the <see cref="IEntityDiff{TEntity}.DatabaseEntities"/> reflects what is the current state in the database.
    /// 
    /// Any relationships that are updated can be retrieved via the methods implemented on 
    /// <see cref="IAffectedRelationships{TDependent}"/>.
    /// </summary>
    public interface IEntityDiff<TEntity> : IAffectedRelationships<TEntity> where TEntity : class, IIdentifiable
    {
        HashSet<TEntity> RequestEntities { get; }
        HashSet<TEntity> DatabaseEntities { get; }
    }

    public class EntityDiff<TEntity> : AffectedRelationships<TEntity>, IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        private readonly HashSet<TEntity> _databaseEntities;
        public HashSet<TEntity> DatabaseEntities { get => _databaseEntities ?? ThrowNoDbValuesError(); }

        public HashSet<TEntity> RequestEntities { get; private set; }
        public EntityDiff(IEnumerable requestEntities,
                          IEnumerable databaseEntities,
                          Dictionary<RelationshipProxy, IEnumerable> relationships) : base(relationships)
        {
            RequestEntities = (HashSet<TEntity>)requestEntities;
            _databaseEntities = (HashSet<TEntity>)databaseEntities;
        }

        private HashSet<TEntity> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot access database entities if the LoadDatabaseValues option is set to false");
        }
    }
}
