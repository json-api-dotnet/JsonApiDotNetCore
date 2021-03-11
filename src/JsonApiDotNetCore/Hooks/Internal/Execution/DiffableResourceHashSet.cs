using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    [PublicAPI]
    public sealed class DiffableResourceHashSet<TResource> : ResourceHashSet<TResource>, IDiffableResourceHashSet<TResource>
        where TResource : class, IIdentifiable
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly CollectionConverter CollectionConverter = new CollectionConverter();

        private readonly HashSet<TResource> _databaseValues;
        private readonly bool _databaseValuesLoaded;
        private readonly IDictionary<PropertyInfo, HashSet<TResource>> _updatedAttributes;

        public DiffableResourceHashSet(HashSet<TResource> requestResources, HashSet<TResource> databaseResources,
            IDictionary<RelationshipAttribute, HashSet<TResource>> relationships, IDictionary<PropertyInfo, HashSet<TResource>> updatedAttributes)
            : base(requestResources, relationships)
        {
            _databaseValues = databaseResources;
            _databaseValuesLoaded |= _databaseValues != null;
            _updatedAttributes = updatedAttributes;
        }

        /// <summary>
        /// Used internally by the ResourceHookExecutor to make live a bit easier with generics
        /// </summary>
        internal DiffableResourceHashSet(IEnumerable requestResources, IEnumerable databaseResources,
            IDictionary<RelationshipAttribute, IEnumerable> relationships, ITargetedFields targetedFields)
            : this((HashSet<TResource>)requestResources, (HashSet<TResource>)databaseResources,
                relationships.ToDictionary(pair => pair.Key, pair => (HashSet<TResource>)pair.Value),
                targetedFields.Attributes?.ToDictionary(attr => attr.Property, _ => (HashSet<TResource>)requestResources))
        {
        }

        /// <inheritdoc />
        public IEnumerable<ResourceDiffPair<TResource>> GetDiffs()
        {
            if (!_databaseValuesLoaded)
            {
                ThrowNoDbValuesError();
            }

            foreach (TResource resource in this)
            {
                TResource currentValueInDatabase = _databaseValues.Single(databaseResource => resource.StringId == databaseResource.StringId);
                yield return new ResourceDiffPair<TResource>(resource, currentValueInDatabase);
            }
        }

        /// <inheritdoc />
        public override HashSet<TResource> GetAffected(Expression<Func<TResource, object>> navigationAction)
        {
            ArgumentGuard.NotNull(navigationAction, nameof(navigationAction));

            PropertyInfo propertyInfo = HooksNavigationParser.ParseNavigationExpression(navigationAction);
            Type propertyType = propertyInfo.PropertyType;

            if (propertyType.IsOrImplementsInterface(typeof(IEnumerable)))
            {
                propertyType = CollectionConverter.TryGetCollectionElementType(propertyType);
            }

            if (propertyType.IsOrImplementsInterface(typeof(IIdentifiable)))
            {
                // the navigation action references a relationship. Redirect the call to the relationship dictionary.
                return base.GetAffected(navigationAction);
            }

            return _updatedAttributes.TryGetValue(propertyInfo, out HashSet<TResource> resources) ? resources : new HashSet<TResource>();
        }

        private void ThrowNoDbValuesError()
        {
            throw new MemberAccessException($"Cannot iterate over the diffs if the ${nameof(LoadDatabaseValuesAttribute)} option is set to false");
        }
    }
}
