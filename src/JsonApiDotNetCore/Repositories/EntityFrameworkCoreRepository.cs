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

namespace JsonApiDotNetCore.Repositories
{
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
        { }
    }

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
        public virtual async Task CreateAsync(TResource resource)
        {
            _traceWriter.LogMethodStart(new {resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            foreach (var relationship in _targetedFields.Relationships)
            {
                var rightValue = relationship.GetValue(resource);
                await ApplyRelationshipUpdate(relationship, resource, rightValue);
            }

            _dbContext.Set<TResource>().Add(resource);
            await SaveChangesAsync();

            FlushFromCache(resource);

            // This ensures relationships get reloaded from the database if they have
            // been requested. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343.
            DetachRelationships(resource);
        }

        /// <inheritdoc />
        public virtual async Task AddToToManyRelationshipAsync(TId id, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = _targetedFields.Relationships.Single();
            var primaryResource = (TResource)_dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

            if (relationship is HasManyThroughAttribute hasManyThroughRelationship)
            {
                // In the case of many-to-many relationships, creating a duplicate entry in the join table results in a uniqueness constraint violation.
                await RemoveAlreadyRelatedResourcesFromAssignment(hasManyThroughRelationship, primaryResource.Id, secondaryResourceIds);
            }

            if (secondaryResourceIds.Any())
            {
                await ApplyRelationshipUpdate(relationship, primaryResource, secondaryResourceIds);
                await SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TId id, object secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});

            var relationship = _targetedFields.Relationships.Single();
            TResource primaryResource = (TResource) _dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));
            
            await EnableCompleteReplacement(relationship, primaryResource);
            await ApplyRelationshipUpdate(relationship, primaryResource, secondaryResourceIds);
            
            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase)
        {
            _traceWriter.LogMethodStart(new {resourceFromRequest, resourceFromDatabase});
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceFromDatabase == null) throw new ArgumentNullException(nameof(resourceFromDatabase));

            // A database entity might not be tracked if it was retrieved through projection.
            resourceFromDatabase = (TResource)_dbContext.GetTrackedOrAttach(resourceFromDatabase);
            
            foreach (var relationship in _targetedFields.Relationships)
            {
                await EnableCompleteReplacement(relationship, resourceFromDatabase);

                var rightResources = relationship.GetValue(resourceFromRequest);
                await ApplyRelationshipUpdate(relationship, resourceFromDatabase, rightResources);
            }

            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceFromDatabase, attribute.GetValue(resourceFromRequest));
            }

            await SaveChangesAsync();
            
            FlushFromCache(resourceFromDatabase);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            var resource = _dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));
            _dbContext.Remove(resource);

            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task RemoveFromToManyRelationshipAsync(TId id, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = (HasManyAttribute)_targetedFields.Relationships.Single();
            var primaryResource = (TResource)_dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

            await EnableCompleteReplacement(relationship, primaryResource);
            
            var rightResources = relationship.GetManyValue(primaryResource, _resourceFactory).ToHashSet(IdentifiableComparer.Instance);
            var currentRightResourcesCount = rightResources.Count;

            rightResources.ExceptWith(secondaryResourceIds);
            
            // TODO: with introduction of HasManyAttribute.GetManyValue, I think we don't have to abstract away substraction of two sets any longer.
            // var newRightResources = GetResourcesToAssignForRemoveFromToManyRelationship(existingRightResources, secondaryResourceIds);

            // todo: why has it been reverted to != again?
            bool hasChanges = rightResources.Count != currentRightResourcesCount;
            if (hasChanges)
            {
                await ApplyRelationshipUpdate(relationship, primaryResource, rightResources);
                await SaveChangesAsync();
            }
        }

        // TODO: Restore or remove commented-out code.
        /*
        /// <summary>
        /// Removes resources from <paramref name="existingRightResources"/> whose ID exists in <paramref name="resourcesToRemove"/>.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// existingRightResources = { 1, 2, 3 }
        /// resourcesToRemove = { 3, 4, 5 }
        /// returns { 1, 2 }
        /// ]]></code>
        /// </example>
        private ICollection<IIdentifiable> GetResourcesToAssignForRemoveFromToManyRelationship(
            ISet<IIdentifiable> existingRightResources, ISet<IIdentifiable> resourcesToRemove)
        {
            var newRightResources = new HashSet<IIdentifiable>(existingRightResources, IdentifiableComparer.Instance);
            newRightResources.ExceptWith(resourcesToRemove);

            return newRightResources;
        }
        */

        private async Task SaveChangesAsync()
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

        private async Task ApplyRelationshipUpdate(RelationshipAttribute relationship, TResource leftResource, object valueToAssign)
        {
            // Ensures the new relationship assignment will not result in entities being tracked more than once.
            var trackedValueToAssign = EnsureRelationshipValueToAssignIsTracked(valueToAssign, relationship.Property.PropertyType);
    
            if (ShouldLoadInverseRelationship(relationship, trackedValueToAssign))
            {
                var entityEntry = _dbContext.Entry(trackedValueToAssign); 
                var inversePropertyName = relationship.InverseNavigationProperty.Name;
                await entityEntry.Reference(inversePropertyName).LoadAsync();
            }
            
            if (HasForeignKeyAtLeftSide(relationship) && trackedValueToAssign == null)
            {
                PrepareChangeTrackerForNullAssignment(relationship, leftResource);
            }
            
            relationship.SetValue(leftResource, trackedValueToAssign, _resourceFactory);
        }

        private bool HasForeignKeyAtLeftSide(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute)
            {
                var entityType = _dbContext.Model.FindEntityType(typeof(TResource));
                var navigationMetadata = entityType.FindNavigation(relationship.Property.Name);
    
                return navigationMetadata.IsDependentToPrincipal();
            }

            return false;
        }

        private TResource CreatePrimaryResourceWithAssignedId(TId id)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return resource;
        }
        
        /// <summary>
        /// Prepares a relationship for complete replacement.
        /// </summary>
        /// <remarks>
        /// For example: a person `p1` has 2 todo-items: `t1` and `t2`.
        /// If we want to update this set to `t3` and `t4`, simply assigning
        /// `p1.todoItems = [t3, t4]` will result in EF Core adding them to the set,
        /// resulting in `[t1 ... t4]`. Instead, we should first include `[t1, t2]`,
        /// after which the reassignment `p1.todoItems = [t3, t4]` will actually 
        /// make EF Core perform a complete replacement. This method does the loading of `[t1, t2]`.
        /// </remarks>
        protected async Task EnableCompleteReplacement(RelationshipAttribute relationship, TResource resource)
        {            
            _traceWriter.LogMethodStart(new {relationship, resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));

            // If the left resource is the dependent side of the relationship, complete replacement is already guaranteed.
            if (!HasForeignKeyAtLeftSide(relationship))
            {
                var navigationEntry = GetNavigationEntryForRelationship(relationship, resource);
                await navigationEntry.LoadAsync();
            }
        }

        private void FlushFromCache(IIdentifiable resource)
        {
            var trackedResource = _dbContext.GetTrackedIdentifiable(resource);
            _dbContext.Entry(trackedResource).State = EntityState.Detached;
        }

        private async Task RemoveAlreadyRelatedResourcesFromAssignment(HasManyThroughAttribute hasManyThroughRelationship, TId primaryResourceId, ISet<IIdentifiable> secondaryResourceIds)
        {
            // TODO: This is a no-go, because it loads the complete set of related entities, which can be massive.
            // Instead, it should only load the subset of related entities that is in secondaryResourceIds, and the deduce what still needs to be added.
            
            // => What you're describing is not possible. We need filtered includes for that, which land in EF Core 5.

            var primaryResource = await _dbContext.Set<TResource>()
                // TODO: Why use AsNoTracking() here? We're not doing that anywhere else.
                .AsNoTracking()
                .Where(r => r.Id.Equals(primaryResourceId))
                .Include(hasManyThroughRelationship.ThroughPropertyName)
                .FirstAsync();

            var existingRightResources = hasManyThroughRelationship.GetManyValue(primaryResource, _resourceFactory).ToHashSet(IdentifiableComparer.Instance);
            secondaryResourceIds.ExceptWith(existingRightResources);
        }

        private NavigationEntry GetNavigationEntryForRelationship(RelationshipAttribute relationship, TResource resource)
        {
            EntityEntry<TResource> entityEntry = _dbContext.Entry(resource);

            switch (relationship)
            {
                case HasManyThroughAttribute hasManyThroughRelationship:
                {
                    return entityEntry.Collection(hasManyThroughRelationship.ThroughProperty.Name);
                }
                case HasManyAttribute hasManyRelationship:
                {
                    return entityEntry.Collection(hasManyRelationship.Property.Name);
                }
                case HasOneAttribute hasOneRelationship:
                {
                    return entityEntry.Reference(hasOneRelationship.Property.Name);
                }
            }

            return null;
        }

        /// <summary>
        /// See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
        /// </summary>
        private bool ShouldLoadInverseRelationship(RelationshipAttribute relationship, object trackedValueToAssign)
        {
            return trackedValueToAssign != null && relationship.InverseNavigationProperty != null && IsOneToOneRelationship(relationship);
        }

        private bool IsOneToOneRelationship(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                var elementType = TypeHelper.TryGetCollectionElementType(hasOneRelationship.InverseNavigationProperty.PropertyType);
                return elementType == null;
            }

            return false;
        }

        /// <summary>
        /// If a (shadow) foreign key is already loaded on the left resource of a relationship, it is not possible to
        /// set it to null by just assigning null to the navigation property and marking it as modified.
        /// Instead, when marking it as modified, it will mark the pre-existing foreign key value as modified too but without setting its value to null.
        /// One way to work around this is by loading the relationship before setting it to null. Another approach (as done in this method) is
        /// tricking the change tracker into recognizing the null assignment by first assigning a placeholder entity to the navigation property, and then
        /// setting it to null.
        /// </summary>
        private void PrepareChangeTrackerForNullAssignment(RelationshipAttribute relationship, TResource leftResource)
        {
            var placeholderRightResource = _resourceFactory.CreateInstance(relationship.RightType);

            // When assigning a related entity to a navigation property, it will be attached to the change tracker.
            // This fails when that entity has null reference(s) for its primary key(s).
            EnsurePrimaryKeyPropertiesAreNotNull(placeholderRightResource);

            relationship.SetValue(leftResource, placeholderRightResource, _resourceFactory);
            _dbContext.Entry(leftResource).DetectChanges();
            
            _dbContext.Entry(placeholderRightResource).State = EntityState.Detached;
        }

        private void EnsurePrimaryKeyPropertiesAreNotNull(object entity)
        {
            var primaryKey = _dbContext.Entry(entity).Metadata.FindPrimaryKey();
            if (primaryKey != null)
            {
                foreach (var property in primaryKey.Properties)
                {
                    var propertyValue = TryGetValueForProperty(property.PropertyInfo);
                    if (propertyValue != null)
                    {
                        property.PropertyInfo.SetValue(entity, propertyValue);
                    }
                }
            }
        }

        private object TryGetValueForProperty(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            
            if (propertyType == typeof(string))
            {
                return string.Empty;
            }

            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                var underlyingType = propertyInfo.PropertyType.GetGenericArguments()[0];

                return Activator.CreateInstance(underlyingType);
            }

            if (!propertyType.IsValueType)
            {
                throw new InvalidOperationException($"Unexpected reference type '{propertyType.Name}' for primary key property '{propertyInfo.Name}'.");
            }

            return null;
        }

        private object EnsureRelationshipValueToAssignIsTracked(object valueToAssign, Type relationshipPropertyType)
        {
            if (valueToAssign is IReadOnlyCollection<IIdentifiable> rightResourcesInToManyRelationship)
            {
                return EnsureToManyRelationshipValueToAssignIsTracked(rightResourcesInToManyRelationship, relationshipPropertyType);
            }

            if (valueToAssign is IIdentifiable rightResourceInToOneRelationship)
            {
                return _dbContext.GetTrackedOrAttach(rightResourceInToOneRelationship);
            }

            return null;
        }

        private IEnumerable EnsureToManyRelationshipValueToAssignIsTracked(IReadOnlyCollection<IIdentifiable> rightResources, Type rightCollectionType)
        {
            var rightResourcesTracked = new object[rightResources.Count];

            int index = 0;
            foreach (var rightResource in rightResources)
            {
                rightResourcesTracked[index] = _dbContext.GetTrackedOrAttach(rightResource);
                index++;
            }

            return TypeHelper.CopyToTypedCollection(rightResourcesTracked, rightCollectionType);
        }

        private void DetachRelationships(TResource resource)
        {
            foreach (var relationship in _targetedFields.Relationships)
            {
                var rightValue = relationship.GetValue(resource);

                if (rightValue is IEnumerable<IIdentifiable> rightResources)
                {
                    foreach (var rightResource in rightResources)
                    {
                        _dbContext.Entry(rightResource).State = EntityState.Detached;
                    }
                }
                else if (rightValue != null)
                {
                    _dbContext.Entry(rightValue).State = EntityState.Detached;
                }
            }
        }
    }
}
