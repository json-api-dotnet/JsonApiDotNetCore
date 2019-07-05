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
    /// implementation of IRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    public interface IEntityHashSetDiff<TResource> : IEntityHashSet<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Iterates over diffs, which is the affected entity from the request
        ///  with their associated current value from the database.
        /// </summary>
        IEnumerator<EntityDiffPair<TResource>> GetDiffs();
            
    }

    /// <inheritdoc />
    public class EntityHashSetDiff<TResource> : EntityHashSet<TResource>, IEntityHashSetDiff<TResource> where TResource : class, IIdentifiable
    {
        private readonly HashSet<TResource> _databaseValues;
        private readonly bool _databaseValuesLoaded;

        public EntityHashSetDiff(HashSet<TResource> requestEntities,
                          HashSet<TResource> databaseEntities,
                          Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) 
            : base(requestEntities, relationships)
        {
            _databaseValues = databaseEntities;
            _databaseValuesLoaded |= _databaseValues != null;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal EntityHashSetDiff(IEnumerable requestEntities,
                  IEnumerable databaseEntities,
                  Dictionary<RelationshipAttribute, IEnumerable> relationships)
            : this((HashSet<TResource>)requestEntities, (HashSet<TResource>)databaseEntities, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }


        /// <inheritdoc />
        public IEnumerator<EntityDiffPair<TResource>> GetDiffs()
        {
            if (!_databaseValuesLoaded) ThrowNoDbValuesError();

            foreach (var entity in this)
            {
                TResource currentValueInDatabase = _databaseValues.Single(e => entity.StringId == e.StringId);
                yield return new EntityDiffPair<TResource>(entity, currentValueInDatabase);
            }
        }

        private HashSet<TResource> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot iterate over the diffs if the LoadDatabaseValues option is set to false");
        }
    }

    /// <summary>
    /// A wrapper that contains an entity that is affected by the request, 
    /// matched to its current database value
    /// </summary>
    public class EntityDiffPair<TResource> where TResource : class, IIdentifiable
    {
        public EntityDiffPair(TResource entity, TResource databaseValue)
        {
            Entity = entity;
            DatabaseValue = databaseValue;
        }

        /// <summary>
        /// The resource from the request matching the resource from the database.
        /// </summary>
        public TResource Entity { get; private set; }
        /// <summary>
        /// The resource from the database matching the resource from the request.
        /// </summary>
        public TResource DatabaseValue { get; private set; }
    }
}
