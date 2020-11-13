using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

// TODO: @ThisPR Tests that cover relationship updates with required relationships. All relationships right now are currently optional.
//    - Setting a required relationship to null
//    - Creating resource with resource
//    - One-to-one required / optional => what is the current behavior?
// tangent:
//     - How and where to read EF Core metadata when "required-relationship-error" is triggered?
namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses Entity Framework Core.
    /// </summary>
    public class EntityFrameworkCoreRepository<TResource, TId> : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ITargetedFields _targetedFields;
        private readonly DbContext _dbContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>> _traceWriter;

        public EntityFrameworkCoreRepository(
            ITargetedFields targetedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            ILoggerFactory loggerFactory)
        {
            if (contextResolver == null) throw new ArgumentNullException(nameof(contextResolver));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));

            _dbContext = contextResolver.GetContext();
            _traceWriter = new TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>>(loggerFactory);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(QueryLayer layer)
        {
            _traceWriter.LogMethodStart(new {layer});
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            IQueryable<TResource> query = ApplyQueryLayer(layer);
            return await query.ToListAsync();
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(FilterExpression topFilter)
        {
            _traceWriter.LogMethodStart(new {topFilter});

            var resourceContext = _resourceGraph.GetResourceContext<TResource>();
            var layer = new QueryLayer(resourceContext)
            {
                Filter = topFilter
            };

            IQueryable<TResource> query = ApplyQueryLayer(layer);
            return await query.CountAsync();
        }

        protected virtual IQueryable<TResource> ApplyQueryLayer(QueryLayer layer)
        {
            _traceWriter.LogMethodStart(new {layer});
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            if (EntityFrameworkCoreSupport.Version.Major < 5)
            {
                var writer = new MemoryLeakDetectionBugRewriter();
                layer = writer.Rewrite(layer);
            }

            IQueryable<TResource> source = GetAll();

            var queryableHandlers = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Where(expressionInScope => expressionInScope.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<QueryableHandlerExpression>()
                .ToArray();

            foreach (var queryableHandler in queryableHandlers)
            {
                source = queryableHandler.Apply(source);
            }

            var nameFactory = new LambdaParameterNameFactory();
            var builder = new QueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory, _resourceGraph, _dbContext.Model);

            var expression = builder.ApplyQuery(layer);
            return source.Provider.CreateQuery<TResource>(expression);
        }

        protected virtual IQueryable<TResource> GetAll()
        {
            return _dbContext.Set<TResource>();
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<object>> GetFromJoinTableAsync(Type entityType, QueryLayer layer)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            IQueryable query = ApplyJoinTableQueryLayer(entityType, layer);

            // We need to prevent these from entering the change tracker, so that when changes are saved
            // in AddToToManyRelationshipAsync(), only new records are added (without deleting existing ones).
            var noTrackingQuery = query.Cast<object>().AsNoTracking();

            return await noTrackingQuery.ToListAsync();
        }

        protected virtual IQueryable ApplyJoinTableQueryLayer(Type entityType, QueryLayer layer)
        {
            var source = _dbContext.Set(entityType);

            var nameFactory = new LambdaParameterNameFactory();
            var builder = new QueryableBuilder(source.Expression, source.ElementType, typeof(Queryable), nameFactory, _resourceFactory, _resourceGraph, _dbContext.Model);

            var expression = builder.ApplyQuery(layer);
            return source.Provider.CreateQuery(expression);
        }

        /// <inheritdoc />
        public virtual async Task CreateAsync(TResource resource)
        {
            _traceWriter.LogMethodStart(new {resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);

            foreach (var relationship in _targetedFields.Relationships)
            {
                var rightValue = relationship.GetValue(resource);
                await UpdateRelationshipAsync(relationship, resource, rightValue, collector);
            }

            var dbSet = _dbContext.Set<TResource>();
            dbSet.Add(resource);

            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task AddToToManyRelationshipAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {primaryId, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = _targetedFields.Relationships.Single();

            if (secondaryResourceIds.Any())
            {
                using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
                var primaryResource = collector.CreateForId<TResource, TId>(primaryId);

                await UpdateRelationshipAsync(relationship, primaryResource, secondaryResourceIds, collector);

                await SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TResource primaryResource, object secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {primaryResource, secondaryResourceIds});

            var relationship = _targetedFields.Relationships.Single();

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            await UpdateRelationshipAsync(relationship, primaryResource, secondaryResourceIds, collector);

            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase)
        {
            _traceWriter.LogMethodStart(new {resourceFromRequest, resourceFromDatabase});
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceFromDatabase == null) throw new ArgumentNullException(nameof(resourceFromDatabase));

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);

            foreach (var relationship in _targetedFields.Relationships)
            {
                var rightResources = relationship.GetValue(resourceFromRequest);
                await UpdateRelationshipAsync(relationship, resourceFromDatabase, rightResources, collector);
            }

            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceFromDatabase, attribute.GetValue(resourceFromRequest));
            }

            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            var resource = collector.CreateForId<TResource, TId>(id);

            foreach (var relationship in _resourceGraph.GetRelationships<TResource>())
            {
                // Loads the data of the relationship, if in EF Core it is configured in such a way that loading the related
                // entities into memory is required for successfully executing the selected deletion behavior. 
                if (RequiresLoadOfRelationshipForDeletion(relationship))
                {
                    var navigation = GetNavigationEntry(resource, relationship);
                    await navigation.LoadAsync();
                }
            }

            _dbContext.Remove(resource);

            await SaveChangesAsync();
        }

        private NavigationEntry GetNavigationEntry(TResource resource, RelationshipAttribute relationship)
        {
            EntityEntry<TResource> entityEntry = _dbContext.Entry(resource);

            switch (relationship)
            {
                case HasOneAttribute hasOneRelationship:
                {
                    return entityEntry.Reference(hasOneRelationship.Property.Name);
                }
                case HasManyAttribute hasManyRelationship:
                {
                    return entityEntry.Collection(hasManyRelationship.Property.Name);
                }
                default:
                {
                    throw new InvalidOperationException($"Unknown relationship type '{relationship.GetType().Name}'.");
                }
            }
        }

        private bool RequiresLoadOfRelationshipForDeletion(RelationshipAttribute relationship)
        {
            var navigation = TryGetNavigationForRelationship(relationship);
            bool isClearForeignKeyRequired = navigation?.ForeignKey.DeleteBehavior == DeleteBehavior.ClientSetNull;

            bool isHasOneWithForeignKeyAtLeftSide = IsHasOneWithForeignKeyAtLeftSide(relationship);

            return !isHasOneWithForeignKeyAtLeftSide && isClearForeignKeyRequired;
        }

        private INavigation TryGetNavigationForRelationship(RelationshipAttribute relationship)
        {
            return _dbContext.Model.FindEntityType(typeof(TResource)).FindNavigation(relationship.Property.Name);
        }

        /// <inheritdoc />
        public virtual async Task RemoveFromToManyRelationshipAsync(TResource primaryResource, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {primaryResource, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = (HasManyAttribute) _targetedFields.Relationships.Single();

            var rightValue = relationship.GetValue(primaryResource);
            var rightResources = ((IEnumerable<IIdentifiable>) rightValue).ToHashSet(IdentifiableComparer.Instance);
            rightResources.ExceptWith(secondaryResourceIds);

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            await UpdateRelationshipAsync(relationship, primaryResource, rightResources, collector);

            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task<TResource> GetForUpdateAsync(QueryLayer queryLayer)
        {
            var resources = await GetAsync(queryLayer);
            return resources.FirstOrDefault();
        }

        protected virtual async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new DataStoreUpdateException(exception);
            }
        }

        private async Task UpdateRelationshipAsync(RelationshipAttribute relationship, TResource leftResource,
            object valueToAssign, PlaceholderResourceCollector collector)
        {
            var trackedValueToAssign = EnsureRelationshipValueToAssignIsTracked(valueToAssign, relationship.Property.PropertyType, collector);

            if (RequireLoadOfInverseRelationship(relationship, trackedValueToAssign))
            {
                var entityEntry = _dbContext.Entry(trackedValueToAssign);
                var inversePropertyName = relationship.InverseNavigationProperty.Name;

                await entityEntry.Reference(inversePropertyName).LoadAsync();
            }

            if (IsHasOneWithForeignKeyAtLeftSide(relationship) && trackedValueToAssign == null)
            {
                PrepareChangeTrackerForNullAssignment(relationship, leftResource);
            }

            relationship.SetValue(leftResource, trackedValueToAssign);
        }

        private object EnsureRelationshipValueToAssignIsTracked(object rightValue, Type relationshipPropertyType,
            PlaceholderResourceCollector collector)
        {
            if (rightValue == null)
            {
                return null;
            }

            var rightResources = TypeHelper.ExtractResources(rightValue);
            var rightResourcesTracked = rightResources.Select(collector.CaptureExisting).ToArray();

            return rightValue is IEnumerable
                ? (object) TypeHelper.CopyToTypedCollection(rightResourcesTracked, relationshipPropertyType)
                : rightResourcesTracked.Single();
        }

        private static bool RequireLoadOfInverseRelationship(RelationshipAttribute relationship, object trackedValueToAssign)
        {
            // See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
            return trackedValueToAssign != null && relationship.InverseNavigationProperty != null && IsOneToOneRelationship(relationship);
        }

        private static bool IsOneToOneRelationship(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                var elementType = TypeHelper.TryGetCollectionElementType(hasOneRelationship.InverseNavigationProperty.PropertyType);
                return elementType == null;
            }

            return false;
        }

        private bool IsHasOneWithForeignKeyAtLeftSide(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute)
            {
                var navigation = TryGetNavigationForRelationship(relationship);
                return navigation != null && navigation.IsDependentToPrincipal();
            }

            return false;
        }

        private void PrepareChangeTrackerForNullAssignment(RelationshipAttribute relationship, TResource leftResource)
        {
            // If a (shadow) foreign key is already loaded on the left resource of a relationship, it is not possible to
            // set it to null by just assigning null to the navigation property and marking it as modified.
            // Instead, when marking it as modified, it will mark the pre-existing foreign key value as modified too,
            // but without setting its value to null.
            // One way to work around this is by loading the relationship before setting it to null. Another approach
            // (as done here) is tricking the change tracker into recognizing the null assignment by first
            // assigning a placeholder entity to the navigation property, and then setting it to null.

            using var collector = new PlaceholderResourceCollector(_resourceFactory, _dbContext);
            var placeholderRightResource = collector.CreateUntracked(relationship.RightType);

            // When assigning a related entity to a navigation property, it will be attached to the change tracker.
            // This fails when that entity has null reference(s) for its primary key.
            EnsurePrimaryKeyPropertiesAreNotNull(placeholderRightResource);

            relationship.SetValue(leftResource, placeholderRightResource);
            _dbContext.Entry(leftResource).DetectChanges();
        }

        private void EnsurePrimaryKeyPropertiesAreNotNull(object entity)
        {
            var primaryKey = _dbContext.Entry(entity).Metadata.FindPrimaryKey();
            if (primaryKey != null)
            {
                foreach (var property in primaryKey.Properties)
                {
                    var propertyValue = GetNonNullValueForProperty(property.PropertyInfo);
                    property.PropertyInfo.SetValue(entity, propertyValue);
                }
            }
        }

        private static object GetNonNullValueForProperty(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(string))
            {
                return string.Empty;
            }

            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
            }

            if (propertyType == typeof(Guid))
            {
                return Guid.NewGuid();
            }

            if (CanBeNegative(propertyType))
            {
                return TypeHelper.ConvertType(-1, propertyType);
            }

            if (propertyType.IsValueType)
            {
                return Activator.CreateInstance(propertyType);
            }

            throw new NotSupportedException(
                $"Unsupported reference type '{propertyType.Name}' for primary key property '{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}'.");
        }

        private static bool CanBeNegative(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                   type == typeof(decimal) || type == typeof(double) || type == typeof(float);
        }
    }

    /// <summary>
    /// Implements the foundational repository implementation that uses Entity Framework Core.
    /// </summary>
    public class EntityFrameworkCoreRepository<TResource> : EntityFrameworkCoreRepository<TResource, int>, IResourceRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public EntityFrameworkCoreRepository(
            ITargetedFields targetedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory)
        {
        }
    }
}
