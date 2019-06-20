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
    public interface IAffectedResourcesDiff<TResource> :  IAffectedResources<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// the current database values of the affected resources collection.
        /// </summary>
        HashSet<TResource> DatabaseValues { get; }

        /// <summary>
        /// Matches the resources from the request to the database values that have been loaded
        /// and exposes them in ResourceDiffPair wrapper
        /// </summary>
        IEnumerable<ResourceDiffPair<TResource>> GetDiffs();
    }

    public  class AffectedResourceDiff<TResource> : AffectedResources<TResource>, IAffectedResourcesDiff<TResource> where TResource : class, IIdentifiable
    {
        private readonly HashSet<TResource> _databaseValues;
        private readonly bool _databaseValuesLoaded;
        /// <inheritdoc />
        public HashSet<TResource> DatabaseValues { get => _databaseValues ?? ThrowNoDbValuesError(); }

        public AffectedResourceDiff(HashSet<TResource> requestEntities,
                          HashSet<TResource> databaseEntities,
                          Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) : base(requestEntities, relationships)
        {
            _databaseValues = databaseEntities;
            _databaseValuesLoaded |= _databaseValues != null;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal AffectedResourceDiff(IEnumerable requestEntities,
                  IEnumerable databaseEntities,
                  Dictionary<RelationshipAttribute, IEnumerable> relationships) 
            : this((HashSet<TResource>)requestEntities, (HashSet<TResource>)databaseEntities, ConvertRelationshipDictionary(relationships)) { }

        /// <inheritdoc />
        public IEnumerable<ResourceDiffPair<TResource>> GetDiffs()
        {
            if (!_databaseValuesLoaded) ThrowNoDbValuesError();

            foreach (var entity in Resources)
            {
                TResource currentValueInDatabase = null;
                currentValueInDatabase = _databaseValues.Single(e => entity.StringId == e.StringId);
                yield return new ResourceDiffPair<TResource>(entity, currentValueInDatabase);
            }
        }

        private HashSet<TResource> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot access database entities if the LoadDatabaseValues option is set to false");
        }
    }

    /// <summary>
    /// A wrapper that contains a resource from the request matches to its current database value
    /// </summary>
    public class ResourceDiffPair<TResource> where TResource : class, IIdentifiable
    {
        public ResourceDiffPair(TResource resource, TResource databaseValue)
        {
            Resource = resource;
            DatabaseValue = databaseValue;
        }

        /// <summary>
        /// The resource from the request matching the resource from the database.
        /// </summary>
        public TResource Resource { get; private set; }
        /// <summary>
        /// The resource from the database matching the resource from the request.
        /// </summary>
        public TResource DatabaseValue { get; private set; }
    }
}
