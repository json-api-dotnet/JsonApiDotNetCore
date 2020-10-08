using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Implements the foundational Repository layer in the JsonApiDotNetCore architecture that uses Entity Framework Core.
    /// </summary>
    public class EntityFrameworkCoreRepository<TResource, TId> : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<RelationshipAttribute, PropertyInfo> _foreignKeyCache = new ConcurrentDictionary<RelationshipAttribute, PropertyInfo>();
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
                var relationshipAssignment = relationship.GetValue(resource);
                ApplyRelationshipAssignment(relationshipAssignment, relationship, resource);
            }

            _dbContext.Set<TResource>().Add(resource);
            await _dbContext.SaveChangesAsync();

            FlushFromCache(resource);

            // this ensures relationships get reloaded from the database if they have
            // been requested. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
            DetachRelationships(resource);
        }

        public async Task AddRelationshipAsync(TId id, IReadOnlyCollection<IIdentifiable> newValues)
        {
            _traceWriter.LogMethodStart(new {id, relationshipAssignment = newValues});
            if (newValues == null) throw new ArgumentNullException(nameof(newValues));
            
            var relationship = _targetedFields.Relationships.Single();
            var databaseResource = _dbContext.GetTrackedOrAttachCurrent(_resourceFactory.CreateInstance<TResource>(id));

            ApplyRelationshipAssignment(newValues, relationship, databaseResource);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new QueryExecutionException(exception);
            }
        }

        public async Task SetRelationshipAsync(TId id, object newValues)
        {
            _traceWriter.LogMethodStart(new {id, relationshipAssignment = newValues});

            var relationship = _targetedFields.Relationships.Single();
            var databaseResource = _dbContext.GetTrackedOrAttachCurrent(_resourceFactory.CreateInstance<TResource>(id));
            
            LoadCurrentRelationships(databaseResource, relationship);
            
            ApplyRelationshipAssignment(newValues, relationship, databaseResource);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new QueryExecutionException(exception);
            }
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TResource requestResource, TResource databaseResource)
        {
            _traceWriter.LogMethodStart(new {requestResource, databaseResource});
            if (requestResource == null) throw new ArgumentNullException(nameof(requestResource));
            if (databaseResource == null) throw new ArgumentNullException(nameof(databaseResource));

            foreach (var attribute in _targetedFields.Attributes)
            {
                attribute.SetValue(databaseResource, attribute.GetValue(requestResource));
            }

            foreach (var relationship in _targetedFields.Relationships)
            {
                // A database entity might not be tracked if it was retrieved through projection. 
                databaseResource = _dbContext.GetTrackedOrAttachCurrent(databaseResource);
                
                // Ensures complete replacements of relationships.
                LoadCurrentRelationships(databaseResource, relationship);

                var relationshipAssignment = relationship.GetValue(requestResource);
                ApplyRelationshipAssignment(relationshipAssignment, relationship, databaseResource);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new QueryExecutionException(exception);
            }
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            var resource = _dbContext.GetTrackedOrAttachCurrent(_resourceFactory.CreateInstance<TResource>(id));
            _dbContext.Remove(resource);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new QueryExecutionException(exception);
            }
        }

        public async Task DeleteRelationshipAsync(TId id, IReadOnlyCollection<IIdentifiable> removalValues)
        {
            _traceWriter.LogMethodStart(new {id, removals = removalValues});
            if (removalValues == null) throw new ArgumentNullException(nameof(removalValues));
            
            var relationship = _targetedFields.Relationships.Single();
            var databaseResource = _dbContext.GetTrackedOrAttachCurrent(_resourceFactory.CreateInstance<TResource>(id));
            
            LoadCurrentRelationships(databaseResource, relationship);

            var currentAssignment = ((IReadOnlyCollection<IIdentifiable>) relationship.GetValue(databaseResource));
            var newAssignment = currentAssignment.Where(i => removalValues.All(r => r.StringId != i.StringId)).ToArray();
            
            if (newAssignment.Length < currentAssignment.Count())
            {
                ApplyRelationshipAssignment(newAssignment, relationship, databaseResource);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException exception)
                {
                    throw new QueryExecutionException(exception);
                }
            }
        }

        /// <inheritdoc />
        public virtual void FlushFromCache(TResource resource)
        {
            _traceWriter.LogMethodStart(new {resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            _dbContext.Entry(resource).State = EntityState.Detached;
        }

        /// <summary>
        /// Loads the inverse relationships to prevent foreign key constraints from being violated
        /// to support implicit removes, see https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
        /// <remark>
        /// Consider the following example: 
        /// person.todoItems = [t1,t2] is updated to [t3, t4]. If t3, and/or t4 was
        /// already related to a other person, and these persons are NOT loaded into the 
        /// DbContext, then the query may cause a foreign key constraint. Loading
        /// these "inverse relationships" into the DB context ensures EF core to take
        /// this into account.
        /// </remark>
        /// </summary>
        private void LoadInverseRelationships(object trackedRelationshipAssignment, RelationshipAttribute relationship)
        {
            var inverseNavigation = relationship.InverseNavigation;
            if (inverseNavigation != null && trackedRelationshipAssignment != null)
            {
                if (trackedRelationshipAssignment is IIdentifiable hasOneAssignment)
                {
                    var hasOneAssignmentEntry = _dbContext.Entry(hasOneAssignment);
                    if (IsOneToOne((HasOneAttribute)relationship))
                    {
                        hasOneAssignmentEntry.Reference(inverseNavigation).Load();
                    }
                    else
                    {
                        hasOneAssignmentEntry.Collection(inverseNavigation).Load();
                    }
                }
                else if (!(relationship is HasManyThroughAttribute))
                {
                    foreach (IIdentifiable assignmentElement in (IEnumerable) trackedRelationshipAssignment)
                    {
                        _dbContext.Entry(assignmentElement).Reference(inverseNavigation).Load();
                    }
                }
            }
        }

        private bool IsOneToOne(HasOneAttribute relationship)
        {
            var relationshipType = relationship.RightType;
            var inverseNavigation = relationship.InverseNavigation;
            
            var inverseRelationship = _resourceGraph.GetRelationships(relationshipType).FirstOrDefault(r => r.Property.Name == inverseNavigation);
            if (inverseRelationship != null)
            {
                return inverseRelationship is HasOneAttribute;
            }
            
            // relationshipAttr is null when we don't put a [RelationshipAttribute] on the inverse navigation property.
            // In this case we reflect on the type to figure out what kind of relationship is pointing back.
            var inverseProperty = relationshipType.GetProperty(inverseNavigation).PropertyType;
            var inversePropertyIsEnumerable = TypeHelper.IsOrImplementsInterface(inverseProperty, typeof(IEnumerable));

            return !inversePropertyIsEnumerable;
        }

        private void DetachRelationships(TResource resource)
        {
            foreach (var relationship in _targetedFields.Relationships)
            {
                var value = relationship.GetValue(resource);
                if (value == null)
                    continue;

                if (value is IEnumerable<IIdentifiable> collection)
                {
                    foreach (IIdentifiable single in collection)
                        _dbContext.Entry(single).State = EntityState.Detached;
                    // detaching has many relationships is not sufficient to 
                    // trigger a full reload of relationships: the navigation 
                    // property actually needs to be nulled out, otherwise
                    // EF will still add duplicate instances to the collection
                    relationship.SetValue(resource, null, _resourceFactory);
                }
                else
                {
                    _dbContext.Entry(value).State = EntityState.Detached;
                }
            }
        }

        /// <summary>
        /// Before assigning new relationship values (UpdateAsync), we need to
        /// attach the current database values of the relationship to the dbContext, else 
        /// it will not perform a complete-replace which is required for 
        /// one-to-many and many-to-many.
        /// <para />
        /// For example: a person `p1` has 2 todo-items: `t1` and `t2`.
        /// If we want to update this todo-item set to `t3` and `t4`, simply assigning
        /// `p1.todoItems = [t3, t4]` will result in EF Core adding them to the set,
        /// resulting in `[t1 ... t4]`. Instead, we should first include `[t1, t2]`,
        /// after which the reassignment  `p1.todoItems = [t3, t4]` will actually 
        /// make EF Core perform a complete replace. This method does the loading of `[t1, t2]`.
        /// </summary>
        protected void LoadCurrentRelationships(TResource databaseResource, RelationshipAttribute relationshipAttribute)
        {
            if (databaseResource == null) throw new ArgumentNullException(nameof(databaseResource));
            if (relationshipAttribute == null) throw new ArgumentNullException(nameof(relationshipAttribute));
            
            if (relationshipAttribute is HasManyThroughAttribute throughAttribute)
            {
                _dbContext.Entry(databaseResource).Collection(throughAttribute.ThroughProperty.Name).Load();
            }
            else if (relationshipAttribute is HasManyAttribute hasManyAttribute)
            {
                _dbContext.Entry(databaseResource).Collection(hasManyAttribute.Property.Name).Load();
            }
            else if (relationshipAttribute is HasOneAttribute hasOneAttribute)
            {
                if (GetForeignKeyProperty(hasOneAttribute) == null)
                {    // If the primary resource is the dependent side of a to-one relationship, there is no
                     // need to load the relationship because we can just set the FK.
                    _dbContext.Entry(databaseResource).Reference(hasOneAttribute.Property.Name).Load();
                }
            }
        }

        private void ApplyRelationshipAssignment(object relationshipAssignment, RelationshipAttribute relationship, TResource targetResource = null)
        {
            // Ensures the new relationship assignment will not result entities being tracked more than once.
            var trackedRelationshipAssignment = GetTrackedRelationshipValue(relationship, relationshipAssignment);
            
            // Ensures successful handling of implicit removals of relationships.
            LoadInverseRelationships(trackedRelationshipAssignment, relationship);
            
            var foreignKey = GetForeignKeyProperty(relationship);
            if (foreignKey != null)
            {
                var foreignKeyValue = trackedRelationshipAssignment == null ? null : TypeHelper.GetResourceTypedId((IIdentifiable) trackedRelationshipAssignment);
                foreignKey.SetValue(targetResource, foreignKeyValue);
                if (_dbContext.Entry(targetResource).State != EntityState.Detached)
                {
                    _dbContext.Entry(targetResource).State = EntityState.Modified;
                }
            }

            relationship.SetValue(targetResource, trackedRelationshipAssignment, _resourceFactory);
        }

        private object GetTrackedRelationshipValue(RelationshipAttribute relationship, object relationshipAssignment)
        {
            object trackedRelationshipAssignment;

            if (relationshipAssignment == null)
            {
                trackedRelationshipAssignment = null;
            } 
            else if (relationshipAssignment is IIdentifiable hasOneAssignment)
            {
                trackedRelationshipAssignment = _dbContext.GetTrackedOrAttachCurrent(hasOneAssignment);
            }
            else
            {
                var hasManyAssignment = ((IReadOnlyCollection<IIdentifiable>) relationshipAssignment);
                var collection = new object[hasManyAssignment.Count()];

                for (int i = 0; i < hasManyAssignment.Count; i++)
                {
                    var trackedHasManyElement = _dbContext.GetTrackedOrAttachCurrent(hasManyAssignment.ElementAt(i));
                    
                    // We should recalculate the target type for every iteration because types may vary. This is possible with resource inheritance.
                    var conversionTarget = trackedHasManyElement.GetType();
                    collection[i] = Convert.ChangeType(trackedHasManyElement, conversionTarget);
                }

                trackedRelationshipAssignment = TypeHelper.CopyToTypedCollection(collection, relationship.Property.PropertyType);
            }
            
            return trackedRelationshipAssignment;
        }
    
        private PropertyInfo GetForeignKeyProperty(RelationshipAttribute relationship)
        {
            PropertyInfo foreignKey = null;
            
            if (relationship is HasOneAttribute && !_foreignKeyCache.TryGetValue(relationship, out foreignKey))
            {
                var entityMetadata = _dbContext.Model.FindEntityType(typeof(TResource));
                var foreignKeyMetadata = entityMetadata.FindNavigation(relationship.Property.Name).ForeignKey;
                foreignKey = foreignKeyMetadata.Properties[0].PropertyInfo;
                _foreignKeyCache.TryAdd(relationship, foreignKey);
            }

            if (foreignKey == null || foreignKey.DeclaringType != typeof(TResource))
            {
                return null;
            }
            
            return foreignKey;
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
        { }
    }
}
