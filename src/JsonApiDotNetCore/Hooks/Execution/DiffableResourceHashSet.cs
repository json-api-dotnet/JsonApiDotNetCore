using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
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
    public interface IDiffableResourceHashSet<TResource> : IResourceHashSet<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Iterates over diffs, which is the affected resource from the request
        ///  with their associated current value from the database.
        /// </summary>
        IEnumerable<ResourceDiffPair<TResource>> GetDiffs();

    }

    /// <inheritdoc />
    public sealed class DiffableResourceHashSet<TResource> : ResourceHashSet<TResource>, IDiffableResourceHashSet<TResource> where TResource : class, IIdentifiable
    {
        private readonly HashSet<TResource> _databaseValues;
        private readonly bool _databaseValuesLoaded;
        private readonly Dictionary<PropertyInfo, HashSet<TResource>> _updatedAttributes;

        public DiffableResourceHashSet(HashSet<TResource> requestResources,
                          HashSet<TResource> databaseResources,
                          Dictionary<RelationshipAttribute, HashSet<TResource>> relationships,
                          Dictionary<PropertyInfo, HashSet<TResource>> updatedAttributes)
            : base(requestResources, relationships)
        {
            _databaseValues = databaseResources;
            _databaseValuesLoaded |= _databaseValues != null;
            _updatedAttributes = updatedAttributes;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal DiffableResourceHashSet(IEnumerable requestResources,
                  IEnumerable databaseResources,
                  Dictionary<RelationshipAttribute, IEnumerable> relationships,
                  ITargetedFields targetedFields)
            : this((HashSet<TResource>)requestResources, (HashSet<TResource>)databaseResources, TypeHelper.ConvertRelationshipDictionary<TResource>(relationships),
              TypeHelper.ConvertAttributeDictionary(targetedFields.Attributes, (HashSet<TResource>)requestResources))
        { }


        /// <inheritdoc />
        public IEnumerable<ResourceDiffPair<TResource>> GetDiffs()
        {
            if (!_databaseValuesLoaded) ThrowNoDbValuesError();

            foreach (var resource in this)
            {
                TResource currentValueInDatabase = _databaseValues.Single(e => resource.StringId == e.StringId);
                yield return new ResourceDiffPair<TResource>(resource, currentValueInDatabase);
            }
        }

        /// <inheritdoc />
        public new HashSet<TResource> GetAffected(Expression<Func<TResource, object>> navigationAction)
        {
            var propertyInfo = TypeHelper.ParseNavigationExpression(navigationAction);
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsOrImplementsInterface(typeof(IEnumerable)))
            {
                propertyType = TypeHelper.TryGetCollectionElementType(propertyType);
            }

            if (propertyType.IsOrImplementsInterface(typeof(IIdentifiable)))
            {
                // the navigation action references a relationship. Redirect the call to the relationship dictionary. 
                return base.GetAffected(navigationAction);
            }
            else if (_updatedAttributes.TryGetValue(propertyInfo, out HashSet<TResource> resources))
            {
                return resources;
            }
            return new HashSet<TResource>();
        }

        private void ThrowNoDbValuesError()
        {
            throw new MemberAccessException($"Cannot iterate over the diffs if the ${nameof(LoadDatabaseValuesAttribute)} option is set to false");
        }
    }

    /// <summary>
    /// A wrapper that contains a resource that is affected by the request, 
    /// matched to its current database value
    /// </summary>
    public sealed class ResourceDiffPair<TResource> where TResource : class, IIdentifiable
    {
        public ResourceDiffPair(TResource resource, TResource databaseValue)
        {
            Resource = resource;
            DatabaseValue = databaseValue;
        }

        /// <summary>
        /// The resource from the request matching the resource from the database.
        /// </summary>
        public TResource Resource { get; }
        /// <summary>
        /// The resource from the database matching the resource from the request.
        /// </summary>
        public TResource DatabaseValue { get; }
    }
}
