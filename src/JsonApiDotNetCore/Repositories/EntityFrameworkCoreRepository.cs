using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
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
                await AssignValueToRelationship(relationship, resource, rightValue);
            }

            _dbContext.Set<TResource>().Add(resource);
            await SaveChangesAsync();

            FlushFromCache(resource);

            // This ensures relationships get reloaded from the database if they have
            // been requested. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343.
            DetachRelationships(resource);
        }

        public async Task AddToToManyRelationshipAsync(TId id, IReadOnlyCollection<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = _targetedFields.Relationships.Single();
            var primaryResource = (TResource)_dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

            await AssignValueToRelationship(relationship, primaryResource, secondaryResourceIds);
            await SaveChangesAsync();
        }

        public async Task SetRelationshipAsync(TId id, object secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});

            var relationship = _targetedFields.Relationships.Single();
            TResource primaryResource = (TResource) _dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

            if (!HasForeignKeyAtLeftSide(relationship))
            {
                await LoadRelationship(relationship, primaryResource);
            }

            await AssignValueToRelationship(relationship, primaryResource, secondaryResourceIds);
            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resourceFromRequest, TResource resourceFromDatabase)
        {
            _traceWriter.LogMethodStart(new {resourceFromRequest, resourceFromDatabase});
            if (resourceFromRequest == null) throw new ArgumentNullException(nameof(resourceFromRequest));
            if (resourceFromDatabase == null) throw new ArgumentNullException(nameof(resourceFromDatabase));

            
            // TODO: I believe the comment below does not apply here (anymore). The calling resource service always fetches the entire record.
            // And commenting out the line below still keeps all tests green.
            // Does this comment maybe apply to SetRelationshipAsync()?
            
            // Maurits: We tried moving the update logic to the repo without success. Now that we're keeping 
            // it this (i.e. service doing a repo.GetAsync and then calling repo.UpdateAsync), I think it is good to
            // keep it a repo responsibility to make sure that the provided database resource is actually present in the change tracker
            // because there is no guarantee it is.

            // A database entity might not be tracked if it was retrieved through projection.
            resourceFromDatabase = (TResource)_dbContext.GetTrackedOrAttach(resourceFromDatabase);

            // TODO: Code inside this loop is very similar to SetRelationshipAsync, we should consider to factor this out into a shared method.
            foreach (var relationship in _targetedFields.Relationships)
            {
                if (!HasForeignKeyAtLeftSide(relationship))
                {
                    await LoadRelationship(relationship, resourceFromDatabase);
                }

                var relationshipAssignment = relationship.GetValue(resourceFromRequest);
                await AssignValueToRelationship(relationship, resourceFromDatabase, relationshipAssignment);
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

        public async Task RemoveFromToManyRelationshipAsync(TId id, IReadOnlyCollection<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = _targetedFields.Relationships.Single();
            var primaryResource = (TResource)_dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

            await LoadRelationship(relationship, primaryResource);

            var existingRightResources = (IReadOnlyCollection<IIdentifiable>)relationship.GetValue(primaryResource);
            var newRightResources = GetResourcesToAssignForRemoveFromToManyRelationship(existingRightResources,
                secondaryResourceIds.Select(r => r.StringId));

            if (newRightResources.Count != existingRightResources.Count)
            {
                await AssignValueToRelationship(relationship, primaryResource, newRightResources);
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes resources from <paramref name="existingRightResources"/> whose ID exists in <paramref name="resourceIdsToRemove"/>.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// existingRightResources = { 1, 2, 3 }
        /// resourceIdsToRemove = { 3, 4, 5 }
        /// returns { 1, 2 }
        /// ]]></code>
        /// </example>
        private ICollection<IIdentifiable> GetResourcesToAssignForRemoveFromToManyRelationship(
            IEnumerable<IIdentifiable> existingRightResources, IEnumerable<string> resourceIdsToRemove)
        {
            var newRightResources = new HashSet<IIdentifiable>(existingRightResources);
            newRightResources.RemoveWhere(r => resourceIdsToRemove.Any(stringId => r.StringId == stringId));
            return newRightResources;
        }

        private TResource CreatePrimaryResourceWithAssignedId(TId id)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return resource;
        }

        private void FlushFromCache(TResource resource)
        {
            _traceWriter.LogMethodStart(new {resource});

            // TODO: Check if this change can be reverted (use GetTrackedIdentifiable).
            var trackedResource = _dbContext.GetTrackedOrAttach(resource);
            _dbContext.Entry(trackedResource).State = EntityState.Detached;
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

                    // Detaching to-many relationships is not sufficient to 
                    // trigger a full reload of relationships: the navigation 
                    // property actually needs to be nulled out, otherwise
                    // EF Core will still add duplicate instances to the collection.

                    // TODO: Ensure that a test exists for this. Commenting out the next line still makes all tests succeed.
                    relationship.SetValue(resource, null, _resourceFactory);
                }
                else if (rightValue != null)
                {
                    _dbContext.Entry(rightValue).State = EntityState.Detached;
                }
            }
        }

        /// <summary>
        /// Before assigning new relationship values, we need to attach the current database values
        /// of the relationship to the DbContext, otherwise it will not perform a complete-replace,
        /// which is required for one-to-many and many-to-many.
        /// <para>
        /// For example: a person `p1` has 2 todo-items: `t1` and `t2`.
        /// If we want to update this set to `t3` and `t4`, simply assigning
        /// `p1.todoItems = [t3, t4]` will result in EF Core adding them to the set,
        /// resulting in `[t1 ... t4]`. Instead, we should first include `[t1, t2]`,
        /// after which the reassignment `p1.todoItems = [t3, t4]` will actually 
        /// make EF Core perform a complete replace. This method does the loading of `[t1, t2]`.
        /// </para>
        /// </summary>
        protected async Task LoadRelationship(RelationshipAttribute relationship, TResource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));

            var navigationEntry = GetNavigationEntryForRelationship(relationship, resource);
            if (navigationEntry != null)
            {
                await navigationEntry.LoadAsync();
            }
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
        /// Loads the inverse of a one-to-one relationship, to support an implicit remove. This prevents a foreign key constraint from being violated.
        /// See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
        /// </summary>
        private async Task LoadInverseForOneToOneRelationship(RelationshipAttribute relationship, object resource)
        {
            var entityEntry = _dbContext.Entry(resource); 
            await entityEntry.Reference(relationship.InverseNavigationProperty.Name).LoadAsync();
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

        private async Task AssignValueToRelationship(RelationshipAttribute relationship, TResource leftResource,
            object valueToAssign)
        {
            // Ensures the new relationship assignment will not result in entities being tracked more than once.
            var trackedValueToAssign = EnsureRelationshipValueToAssignIsTracked(valueToAssign, relationship.Property.PropertyType);
            if (trackedValueToAssign != null && relationship.InverseNavigationProperty != null && IsOneToOneRelationship(relationship))
            {
                await LoadInverseForOneToOneRelationship(relationship, trackedValueToAssign);
            }
            
            if (HasSingleForeignKeyAtLeftSide(relationship))
            {
                var foreignKeyProperty = GetForeignKeyProperties((HasOneAttribute)relationship).First();
                SetValueThroughForeignKeyProperty(foreignKeyProperty, leftResource, valueToAssign);
            }
            else
            {
                relationship.SetValue(leftResource, trackedValueToAssign, _resourceFactory);
                if (trackedValueToAssign == null)
                {
                    var entry = GetNavigationEntryForRelationship(relationship, leftResource);
                    entry.IsModified = true;
                }
            }
        }
        
        private void SetValueThroughForeignKeyProperty(IProperty foreignKeyProperty, TResource leftResource, object valueToAssign)
        {
            var rightResourceId = valueToAssign is IIdentifiable rightResource
                ? rightResource.GetTypedId()
                : null;

            // https://stackoverflow.com/questions/10257360/how-to-update-not-every-fields-of-an-object-using-entity-framework-and-entitysta
            var entityEntry = _dbContext.Entry(leftResource);
            entityEntry.Property(foreignKeyProperty.Name).CurrentValue = rightResourceId;
            entityEntry.Property(foreignKeyProperty.Name).IsModified = true;
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

        private bool HasSingleForeignKeyAtLeftSide(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                var foreignKeyProperties = GetForeignKeyProperties(hasOneRelationship);
                var hasForeignKeyOnLeftSide = foreignKeyProperties.First().DeclaringType.ClrType == typeof(TResource);
                var hasSingleForeignKey = foreignKeyProperties.Count == 1;
                return hasForeignKeyOnLeftSide && hasSingleForeignKey;                
            }

            return false;
        }
        
        private bool HasForeignKeyAtLeftSide(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                var foreignKeyProperties = GetForeignKeyProperties(hasOneRelationship);
                var hasForeignKeyOnLeftSide = foreignKeyProperties.First().DeclaringType.ClrType == typeof(TResource);
                return hasForeignKeyOnLeftSide;
            }

            return false;
        }

        private IReadOnlyList<IProperty> GetForeignKeyProperties(HasOneAttribute relationship)
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(TResource));
            var foreignKeyMetadata = entityType.FindNavigation(relationship.Property.Name).ForeignKey;
            
            return foreignKeyMetadata.Properties;
        }

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
    }
}
