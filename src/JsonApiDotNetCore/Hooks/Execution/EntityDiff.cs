using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class that provides insight in what is to be updated. The 
    /// <see cref="IResourceDiff{TEntity}.RequestEntities"/> property reflects what was parsed from the incoming request,
    /// where the <see cref="IResourceDiff{TEntity}.DatabaseValues"/> reflects what is the current state in the database.
    /// 
    /// Any relationships that are updated can be retrieved via the methods implemented on 
    /// <see cref="IAffectedRelationships{TDependent}"/>.
    /// </summary>
    public interface IResourceDiff<TEntity> : IEnumerable<ResourceDiffPair<TEntity>>, IAffectedResourcesBase<TEntity> where TEntity : class, IIdentifiable
    {
        HashSet<TEntity> DatabaseValues { get; }
    }

    public  class ResourceDiff<TEntity> : AffectedResourcesBase<TEntity>, IResourceDiff<TEntity> where TEntity : class, IIdentifiable
    {
        private readonly HashSet<TEntity> _databaseEntities;
        private readonly bool _databaseValuesLoaded;
        public HashSet<TEntity> DatabaseValues { get => _databaseEntities ?? ThrowNoDbValuesError(); }

        internal ResourceDiff(IEnumerable requestEntities,
                          IEnumerable databaseEntities,
                          Dictionary<RelationshipProxy, IEnumerable> relationships) : base(requestEntities, relationships)
        {
            _databaseEntities = (HashSet<TEntity>)databaseEntities;
            _databaseValuesLoaded |= _databaseEntities != null;
        }

        private HashSet<TEntity> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot access database entities if the LoadDatabaseValues option is set to false");
        }

        public IEnumerator<ResourceDiffPair<TEntity>> GetEnumerator()
        {
            foreach (var entity in Entities)
            {
                TEntity currentValueInDatabase = null;
                if (_databaseValuesLoaded) currentValueInDatabase = _databaseEntities.Single(e => entity.StringId == e.StringId);
                yield return new ResourceDiffPair<TEntity>(entity, currentValueInDatabase);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ResourceDiffPair<TEntity> where TEntity : class, IIdentifiable
    {
        internal ResourceDiffPair(TEntity entity, TEntity databaseValue)
        {
            Entity = entity;
            DatabaseValue = databaseValue;
        }

        public TEntity Entity { get; private set; }
        public TEntity DatabaseValue { get; private set; }
    }
}
