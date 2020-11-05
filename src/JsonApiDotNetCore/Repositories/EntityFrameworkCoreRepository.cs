using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Repositories.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

// TODO: Tests that cover relationship updates with required relationships. All relationships right are currently optional.
//    - Setting a required relationship to null
//    - Creating resource with resource
//    - One-to-one required / optional => what is the current behavior?
// tangent:
//     - How and where to read EF Core metadata when "required-relationship-error" is triggered?
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
            IGetResourcesByIds getResourcesByIds,
            ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, getResourcesByIds, loggerFactory) 
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
        private readonly IGetResourcesByIds _getResourcesByIds;
        private readonly TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>> _traceWriter;

        public EntityFrameworkCoreRepository(ITargetedFields targetedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IGetResourcesByIds getResourcesByIds,
            ILoggerFactory loggerFactory)
        {
            if (contextResolver == null) throw new ArgumentNullException(nameof(contextResolver));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));
            _getResourcesByIds = getResourcesByIds ?? throw new ArgumentNullException(nameof(getResourcesByIds));
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
        }

        /// <inheritdoc />
        public virtual async Task AddToToManyRelationshipAsync(TId id, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            var relationship = _targetedFields.Relationships.Single();

            if (relationship is HasManyThroughAttribute hasManyThroughRelationship)
            {
                // In the case of many-to-many relationships, creating a duplicate entry in the join table results in a uniqueness constraint violation.
                await RemoveAlreadyRelatedResourcesFromAssignment(hasManyThroughRelationship, id, secondaryResourceIds);
            }
            
            var primaryResource = (TResource)_dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

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

            var primaryResource = await GetPrimaryResourceForCompleteReplacement(id, _targetedFields.Relationships);
            
            var relationship = _targetedFields.Relationships.Single();

            await ApplyRelationshipUpdate(relationship, primaryResource, secondaryResourceIds);
            
            await SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource resource)
        {
            _traceWriter.LogMethodStart(new {resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var resourceFromDatabase = await GetPrimaryResourceForCompleteReplacement(resource.Id, _targetedFields.Relationships);

            foreach (var relationship in _targetedFields.Relationships)
            {
                var rightResources = relationship.GetValue(resource);
                await ApplyRelationshipUpdate(relationship, resourceFromDatabase, rightResources);
            }

            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(resourceFromDatabase, attribute.GetValue(resource));
            }

            await SaveChangesAsync();

            FlushFromCache(resourceFromDatabase);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            var resource = (TResource)_dbContext.GetTrackedOrAttach(CreatePrimaryResourceWithAssignedId(id));

            foreach (var relationship in _resourceGraph.GetRelationships<TResource>())
            {
                if (ShouldLoadRelationshipForSafeDeletion(relationship))
                {
                    var navigation = GetNavigationEntry(resource, relationship);
                    await navigation.LoadAsync();
                }
            }

            _dbContext.Remove(resource);

            await SaveChangesAsync();
        }

        /// <summary>
        /// Loads the data of the relationship if in EF Core it is configured in such a way that loading the related
        /// entities into memory is required for successfully executing the selected deletion behavior. 
        /// </summary>
        private bool ShouldLoadRelationshipForSafeDeletion(RelationshipAttribute relationship)
        {
            var navigationMeta = GetNavigationMetadata(relationship);
            var clientIsResponsibleForClearingForeignKeys = navigationMeta?.ForeignKey.DeleteBehavior == DeleteBehavior.ClientSetNull;

            var isPrincipalSide = !HasForeignKeyAtLeftSide(relationship);

            return isPrincipalSide && clientIsResponsibleForClearingForeignKeys;
        }

        private INavigation GetNavigationMetadata(RelationshipAttribute relationship)
        {
            return _dbContext.Model.FindEntityType(typeof(TResource)).FindNavigation(relationship.Property.Name);
        }

        /// <inheritdoc />
        public virtual async Task RemoveFromToManyRelationshipAsync(TId id, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, secondaryResourceIds});
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));
            
            var primaryResource = await GetPrimaryResourceForCompleteReplacement(id, _targetedFields.Relationships);

            var relationship = (HasManyAttribute)_targetedFields.Relationships.Single();
            await AssertSecondaryResourcesExist(secondaryResourceIds, relationship);

            var rightResources = ((IEnumerable<IIdentifiable>)relationship.GetValue(primaryResource)).ToHashSet(IdentifiableComparer.Instance);
            rightResources.ExceptWith(secondaryResourceIds);
            
            await ApplyRelationshipUpdate(relationship, primaryResource, rightResources);
            await SaveChangesAsync();
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

        private async Task ApplyRelationshipUpdate(RelationshipAttribute relationship, TResource leftResource, object valueToAssign)
        {
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
            
            relationship.SetValue(leftResource, trackedValueToAssign);
        }

        private bool HasForeignKeyAtLeftSide(RelationshipAttribute relationship)
        {
            if (relationship is HasOneAttribute)
            {
                var navigation = GetNavigationMetadata(relationship);
            
                return navigation.IsDependentToPrincipal();
            }

            return false;
        }

        private TResource CreatePrimaryResourceWithAssignedId(TId id)
        {
            var resource = _resourceFactory.CreateInstance<TResource>();
            resource.Id = id;

            return resource;
        }
        
        private void FlushFromCache(IIdentifiable resource)
        {
            resource = (IIdentifiable)_dbContext.GetTrackedIdentifiable(resource);
            if (resource != null)
            {
                DetachEntities(new [] { resource });
                DetachRelationships(resource);
            }
        }

        private async Task RemoveAlreadyRelatedResourcesFromAssignment(HasManyThroughAttribute relationship, TId primaryResourceId, ISet<IIdentifiable> secondaryResourceIds)
        {
            // TODO: Finalize this.
            var throughEntitiesFilter = new ThroughEntitiesFilter(_dbContext, relationship);
            var typedRightIds = secondaryResourceIds.Select(resource => resource.GetTypedId()).ToHashSet();
            var throughEntities = await throughEntitiesFilter.GetBy(primaryResourceId, typedRightIds);
            
            // Alternative approaches:
            // throughEntities = await GetFilteredThroughEntities_DynamicQueryBuilding(hasManyThroughRelationship, primaryResourceId, secondaryResourceIds);
            // throughEntities = await GetFilteredThroughEntities_QueryBuilderCall(hasManyThroughRelationship, primaryResourceId, secondaryResourceIds);
            
            var rightResources = throughEntities.Select(ConstructRightResourceOfHasManyRelationship).ToHashSet();
            secondaryResourceIds.ExceptWith(rightResources.ToHashSet());
            
            DetachEntities(throughEntities);
        }

        private IIdentifiable ConstructRightResourceOfHasManyRelationship(object entity)
        {
            var relationship = (HasManyThroughAttribute)_targetedFields.Relationships.Single();

            var rightResource = _resourceFactory.CreateInstance(relationship.RightType);
            rightResource.StringId = relationship.RightIdProperty.GetValue(entity).ToString();

            return rightResource;
        }

        private async Task<object[]> GetFilteredThroughEntities_DynamicQueryBuilding(TId leftId, ISet<object> rightIds, HasManyThroughAttribute relationship)
        {
            var throughEntityParameter = Expression.Parameter(relationship.ThroughType, relationship.ThroughType.Name.Camelize());
            
            var filter = ThroughEntitiesFilter.GetEqualsAndContainsFilter(leftId, rightIds, relationship, throughEntityParameter);

            var predicate = Expression.Lambda(filter, throughEntityParameter);

            IQueryable throughSource = _dbContext.Set(relationship.ThroughType);
            var whereClause = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { relationship.ThroughType }, throughSource.Expression, predicate);
            
            dynamic query = throughSource.Provider.CreateQuery(whereClause);
            IEnumerable result = await EntityFrameworkQueryableExtensions.ToListAsync(query);
            
            return result.Cast<object>().ToArray();
        }
        
        private async Task<object[]> GetFilteredThroughEntities_QueryBuilderCall(TId leftId, ISet<object> rightIds, HasManyThroughAttribute relationship)
        {
            var comparisonTargetField = new ResourceFieldChainExpression(new AttrAttribute { Property = relationship.LeftIdProperty });
            var comparisionId = new LiteralConstantExpression(leftId.ToString());
            FilterExpression equalsFilter = new ComparisonExpression(ComparisonOperator.Equals, comparisonTargetField, comparisionId);
        
            var equalsAnyOfTargetField = new ResourceFieldChainExpression(new AttrAttribute { Property =  relationship.RightIdProperty });
            var equalsAnyOfIds = rightIds.Select(r => new LiteralConstantExpression(r.ToString())).ToArray();
            FilterExpression containsFilter = new EqualsAnyOfExpression(equalsAnyOfTargetField, equalsAnyOfIds);
            
            var filter = new LogicalExpression(LogicalOperator.And, new QueryExpression[] { equalsFilter, containsFilter } );
            
            IQueryable throughSource = _dbContext.Set(relationship.ThroughType);
        
            var scopeFactory = new LambdaScopeFactory(new LambdaParameterNameFactory());
            var scope = scopeFactory.CreateScope(relationship.ThroughType);
            
            var whereClauseBuilder = new WhereClauseBuilder(throughSource.Expression, scope, typeof(Queryable));
            var whereClause = whereClauseBuilder.ApplyWhere(filter);
        
            dynamic query = throughSource.Provider.CreateQuery(whereClause);
            IEnumerable result = await EntityFrameworkQueryableExtensions.ToListAsync(query);
        
            return result.Cast<object>().ToArray();
        }

        private NavigationEntry GetNavigationEntry(TResource resource, RelationshipAttribute relationship)
        {
            EntityEntry<TResource> entityEntry = _dbContext.Entry(resource);

            switch (relationship)
            {
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

            relationship.SetValue(leftResource, placeholderRightResource);
            _dbContext.Entry(leftResource).DetectChanges();
            
            DetachEntities(new [] { placeholderRightResource });
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
                // TODO: Write test with primary key property type int? or equivalent. 
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

        /// <summary>
        /// Gets the primary resource by ID and performs side-loading of data such that EF Core correctly performs complete replacements of relationships. 
        /// </summary>
        /// <remarks>
        /// For example: a person `p1` has 2 todo-items: `t1` and `t2`.
        /// If we want to update this set to `t3` and `t4`, simply assigning
        /// `p1.todoItems = [t3, t4]` will result in EF Core adding them to the set,
        /// resulting in `[t1 ... t4]`. Instead, we should first include `[t1, t2]`,
        /// after which the reassignment `p1.todoItems = [t3, t4]` will actually 
        /// make EF Core perform a complete replacement. This method does the loading of `[t1, t2]`.
        /// </remarks>
        private async Task<TResource> GetPrimaryResourceForCompleteReplacement(TId id, ISet<RelationshipAttribute> relationships)
        {
            TResource primaryResource;

            if (relationships.Any())
            {
                IQueryable<TResource> query = _dbContext.Set<TResource>();
                foreach (var relationship in relationships)
                {
                    query = query.Include(relationship.RelationshipPath);
                }

                primaryResource = query.FirstOrDefault(resource => resource.Id.Equals(id));
            }
            else
            {
                primaryResource = await _dbContext.FindAsync<TResource>(id);
            }

            if (primaryResource == null)
            {
                var tempResource = _resourceFactory.CreateInstance<TResource>();
                tempResource.Id = id;

                var resourceContext = _resourceGraph.GetResourceContext<TResource>();
                throw new ResourceNotFoundException(tempResource.StringId, resourceContext.PublicName);
            }

            return primaryResource;
        }

        private async Task AssertSecondaryResourcesExist(ISet<IIdentifiable> secondaryResourceIds, HasManyAttribute relationship)
        {
            var typedIds = secondaryResourceIds.Select(resource => resource.GetTypedId()).ToHashSet();
            var secondaryResourcesFromDatabase = await _getResourcesByIds.Get(relationship.RightType, typedIds);

            if (secondaryResourcesFromDatabase.Count < secondaryResourceIds.Count)
            {
                throw new DataStoreUpdateException($"One or more related resources of type '{relationship.RightType}' do not exist.");
            }

            DetachEntities(secondaryResourcesFromDatabase.ToArray());
        }

        private void DetachRelationships(IIdentifiable resource)
        {
            foreach (var relationship in _targetedFields.Relationships)
            {
                var rightValue = relationship.GetValue(resource);

                if (rightValue is IEnumerable<IIdentifiable> rightResources)
                {
                    DetachEntities(rightResources.ToArray());
                }
                else if (rightValue != null)
                {
                    DetachEntities(new [] { rightValue });
                    _dbContext.Entry(rightValue).State = EntityState.Detached;
                }
            }
        }
        
        private void DetachEntities(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }
        }
    }
}
