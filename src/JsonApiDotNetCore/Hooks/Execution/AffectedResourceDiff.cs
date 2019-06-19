using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A helper class that provides insight in what is to be updated. The 
    /// <see cref="IAffectedResourcesDiff{TEntity}.RequestEntities"/> property reflects what was parsed from the incoming request,
    /// where the <see cref="IAffectedResourcesDiff{TEntity}.DatabaseValues"/> reflects what is the current state in the database.
    /// 
    /// Any relationships that are updated can be retrieved via the methods implemented on 
    /// <see cref="IAffectedRelationships{TDependent}"/>.
    /// </summary>
    public interface IAffectedResourcesDiff<TEntity> :  IAffectedResources<TEntity> where TEntity : class, IIdentifiable
    {
        HashSet<TEntity> DatabaseValues { get; }
        IEnumerable<ResourceDiffPair<TEntity>> GetDiff();
    }

    public  class ResourceDiff<TEntity> : AffectedResources<TEntity>, IAffectedResourcesDiff<TEntity> where TEntity : class, IIdentifiable
    {

        private readonly HashSet<TEntity> _databaseValues;
        private readonly bool _databaseValuesLoaded;

        /// <summary>
        /// the current database values of the affected resources collection.
        /// </summary>
        public HashSet<TEntity> DatabaseValues { get => _databaseValues ?? ThrowNoDbValuesError(); }

        public ResourceDiff(IEnumerable requestEntities,
                          IEnumerable databaseEntities,
                          Dictionary<RelationshipProxy, IEnumerable> relationships) : base(requestEntities, relationships)
        {
            _databaseValues = (HashSet<TEntity>)databaseEntities;
            _databaseValuesLoaded |= _databaseValues != null;
        }

        public IEnumerable<ResourceDiffPair<TEntity>> GetDiff()
        {
            foreach (var entity in Entities)
            {
                TEntity currentValueInDatabase = null;
                if (_databaseValuesLoaded) currentValueInDatabase = _databaseValues.Single(e => entity.StringId == e.StringId);
                yield return new ResourceDiffPair<TEntity>(entity, currentValueInDatabase);
            }
        }

        private HashSet<TEntity> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot access database entities if the LoadDatabaseValues option is set to false");
        }
    }

    public class ResourceDiffPair<TEntity> where TEntity : class, IIdentifiable
    {
        public ResourceDiffPair(TEntity entity, TEntity databaseValue)
        {
            Entity = entity;
            DatabaseValue = databaseValue;
        }

        public TEntity Entity { get; private set; }
        public TEntity DatabaseValue { get; private set; }
    }
}
