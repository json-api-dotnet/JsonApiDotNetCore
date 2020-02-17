using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A wrapper class that contains information about the resources that are updated by the request.
    /// Contains the resources from the request and the corresponding database values.
    /// 
    /// Also contains information about updated relationships through 
    /// implementation of IRelationshipsDictionary<typeparamref name="TResource"/>>
    /// </summary>
    public interface IDiffableEntityHashSet<TResource> : IEntityHashSet<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Iterates over diffs, which is the affected entity from the request
        ///  with their associated current value from the database.
        /// </summary>
        IEnumerable<EntityDiffPair<TResource>> GetDiffs();

    }

    /// <inheritdoc />
    public class DiffableEntityHashSet<TResource> : EntityHashSet<TResource>, IDiffableEntityHashSet<TResource> where TResource : class, IIdentifiable
    {
        private readonly HashSet<TResource> _databaseValues;
        private readonly bool _databaseValuesLoaded;
        private readonly Dictionary<PropertyInfo, HashSet<TResource>> _updatedAttributes;

        public DiffableEntityHashSet(HashSet<TResource> requestEntities,
                          HashSet<TResource> databaseEntities,
                          Dictionary<RelationshipAttribute, HashSet<TResource>> relationships,
                          Dictionary<PropertyInfo, HashSet<TResource>> updatedAttributes)
            : base(requestEntities, relationships)
        {
            _databaseValues = databaseEntities;
            _databaseValuesLoaded |= _databaseValues != null;
            _updatedAttributes = updatedAttributes;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal DiffableEntityHashSet(IEnumerable requestEntities,
                  IEnumerable databaseEntities,
                  Dictionary<RelationshipAttribute, IEnumerable> relationships,
                  ITargetedFields targetedFields)
            : this((HashSet<TResource>)requestEntities, (HashSet<TResource>)databaseEntities, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships),
              TypeHelper.ConvertAttributeDictionary(targetedFields.Attributes, (HashSet<TResource>)requestEntities))
        { }


        /// <inheritdoc />
        public IEnumerable<EntityDiffPair<TResource>> GetDiffs()
        {
            if (!_databaseValuesLoaded) ThrowNoDbValuesError();

            foreach (var entity in this)
            {
                TResource currentValueInDatabase = _databaseValues.Single(e => entity.StringId == e.StringId);
                yield return new EntityDiffPair<TResource>(entity, currentValueInDatabase);
            }
        }

        /// <inheritdoc />
        public new HashSet<TResource> GetAffected(Expression<Func<TResource, object>> NavigationAction)
        {
            var propertyInfo = TypeHelper.ParseNavigationExpression(NavigationAction);
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.Inherits(typeof(IEnumerable))) propertyType = TypeHelper.GetTypeOfList(propertyType);
            if (propertyType.Implements<IIdentifiable>())
            {
                // the navigation action references a relationship. Redirect the call to the relationship dictionary. 
                return base.GetAffected(NavigationAction);
            }
            else if (_updatedAttributes.TryGetValue(propertyInfo, out HashSet<TResource> entities))
            {
                return entities;
            }
            return new HashSet<TResource>();
        }

        private void ThrowNoDbValuesError()
        {
            throw new MemberAccessException($"Cannot iterate over the diffs if the ${nameof(LoadDatabaseValues)} option is set to false");
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
        public TResource Entity { get; }
        /// <summary>
        /// The resource from the database matching the resource from the request.
        /// </summary>
        public TResource DatabaseValue { get; }
    }
}
