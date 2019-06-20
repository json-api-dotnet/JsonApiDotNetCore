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
    /// implementation of IAffectedRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    public interface IAffectedResourcesDiffs<TResource> : IRelationshipsDictionary<TResource>, IEnumerable<ResourceDiffPair<TResource>> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// The database values of the resources affected by the request.
        /// </summary>
        HashSet<TResource> DatabaseValues { get; }

        /// <summary>
        /// The resources that were affected by the request.
        /// </summary>
        HashSet<TResource> Resources { get; }
    }

    /// <inheritdoc />
    public class AffectedResourcesDiffs<TResource> : IAffectedResourcesDiffs<TResource> where TResource : class, IIdentifiable
    {
        private readonly HashSet<TResource> _databaseValues;
        private readonly bool _databaseValuesLoaded;

        /// <inheritdoc />
        public HashSet<TResource> DatabaseValues { get => _databaseValues ?? ThrowNoDbValuesError(); }
        /// <inheritdoc />
        public HashSet<TResource> Resources { get; private set; }
        /// <inheritdoc />
        public RelationshipsDictionary<TResource> AffectedRelationships { get; private set; }

        public AffectedResourcesDiffs(HashSet<TResource> requestEntities,
                          HashSet<TResource> databaseEntities,
                          Dictionary<RelationshipAttribute, HashSet<TResource>> relationships) 
        {
            Resources = requestEntities;
            AffectedRelationships = new RelationshipsDictionary<TResource>(relationships);
            _databaseValues = databaseEntities;
            _databaseValuesLoaded |= _databaseValues != null;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal AffectedResourcesDiffs(IEnumerable requestEntities,
                  IEnumerable databaseEntities,
                  Dictionary<RelationshipAttribute, IEnumerable> relationships) 
            : this((HashSet<TResource>)requestEntities, (HashSet<TResource>)databaseEntities, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships)) { }


        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship<TPrincipalResource>() where TPrincipalResource : class, IIdentifiable
        {
            return GetByRelationship(typeof(TPrincipalResource));
        }

        /// <inheritdoc />
        public Dictionary<RelationshipAttribute, HashSet<TResource>> GetByRelationship(Type principalType)
        {
            return AffectedRelationships.GetByRelationship(principalType);
        }

        /// <inheritdoc />
        public IEnumerator<ResourceDiffPair<TResource>> GetEnumerator()
        {
            if (!_databaseValuesLoaded) ThrowNoDbValuesError();

            foreach (var entity in Resources)
            {
                TResource currentValueInDatabase = null;
                currentValueInDatabase = _databaseValues.Single(e => entity.StringId == e.StringId);
                yield return new ResourceDiffPair<TResource>(entity, currentValueInDatabase);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private HashSet<TResource> ThrowNoDbValuesError()
        {
            throw new MemberAccessException("Cannot access database entities if the LoadDatabaseValues option is set to false");
        }
    }

    /// <summary>
    /// A wrapper that contains an resource that is affected by the request, 
    /// matched to its current database value
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
