using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A wrapper class that contains information about the resources that are updated by the request.
    /// Contains the resources from the request and the corresponding database values.
    /// 
    /// Also contains information about updated relationships through 
    /// implementation of IRelationshipsDictionary<typeparamref name="TEntity"/>>
    /// </summary>
    public interface IEntityDiff<TEntity> : IRelationshipsDictionary<TEntity>, IEnumerable<EntityDiffPair<TEntity>> where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// The database values of the resources affected by the request.
        /// </summary>
        HashSet<TEntity> DatabaseValues { get; }

        /// <summary>
        /// The resources that were affected by the request.
        /// </summary>
        HashSet<TEntity> Entities { get; }
    }

    /// <inheritdoc />
    public class EntityDiffs<TEntity> : IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        /// <inheritdoc />
        public HashSet<TEntity> DatabaseValues { get => _databaseValues ?? ThrowNoDbValuesError(); }
        private readonly HashSet<TEntity> _databaseValues;
        private readonly bool _databaseValuesLoaded;

        /// <inheritdoc />
        public HashSet<TEntity> Entities { get; private set; }
        /// <inheritdoc />
        public RelationshipsDictionary<TEntity> AffectedRelationships { get; private set; }

        public EntityDiffs(HashSet<TEntity> requestEntities,
                          HashSet<TEntity> databaseEntities,
                          Dictionary<RelationshipAttribute, HashSet<TEntity>> relationships) 
        {
            Entities = requestEntities;
            AffectedRelationships = new RelationshipsDictionary<TEntity>(relationships);
            _databaseValues = databaseEntities;
            _databaseValuesLoaded |= _databaseValues != null;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal EntityDiffs(IEnumerable requestEntities,
                  IEnumerable databaseEntities,
                  Dictionary<RelationshipAttribute, IEnumerable> relationships) 
            : this((HashSet<TEntity>)requestEntities, (HashSet<TEntity>)databaseEntities, TypeHelper.ConvertRelationshipDictionary<TEntity>(relationships)) { }


        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TEntity>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TPrincipalResource));
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TEntity>> GetByRelationship(Type principalType)
        {
            return AffectedRelationships.GetByRelationship(principalType);
        }

        /// <inheritdoc />
        public IEnumerator<EntityDiffPair<TEntity>> GetEnumerator()
        {
            if (!_databaseValuesLoaded) ThrowNoDbValuesError();

            foreach (var entity in Entities)
            {
                TEntity currentValueInDatabase = null;
                currentValueInDatabase = _databaseValues.Single(e => entity.StringId == e.StringId);
                yield return new EntityDiffPair<TEntity>(entity, currentValueInDatabase);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private HashSet<TEntity> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot access database entities if the LoadDatabaseValues option is set to false");
        }
    }

    /// <summary>
    /// A wrapper that contains an entity that is affected by the request, 
    /// matched to its current database value
    /// </summary>
    public class EntityDiffPair<TEntity> where TEntity : class, IIdentifiable
    {
        public EntityDiffPair(TEntity entity, TEntity databaseValue)
        {
            Entity = entity;
            DatabaseValue = databaseValue;
        }

        /// <summary>
        /// The resource from the request matching the resource from the database.
        /// </summary>
        public TEntity Entity { get; private set; }
        /// <summary>
        /// The resource from the database matching the resource from the request.
        /// </summary>
        public TEntity DatabaseValue { get; private set; }
    }
}
