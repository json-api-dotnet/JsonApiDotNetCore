using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
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
        private readonly ITargetedFields _targetedFields;
        private readonly DbContext _dbContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceFactory _resourceFactory;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceAccessor _resourceAccessor;
        private readonly TraceLogWriter<EntityFrameworkCoreRepository<TResource, TId>> _traceWriter;

        public EntityFrameworkCoreRepository(
            ITargetedFields targetedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IResourceFactory resourceFactory,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceAccessor resourceAccessor,
            ILoggerFactory loggerFactory)
        {
            if (contextResolver == null) throw new ArgumentNullException(nameof(contextResolver));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));
            _resourceAccessor = resourceAccessor ?? throw new ArgumentNullException(nameof(constraintProviders));
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
            
            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                var relationshipIds = GetRelationshipIds(relationshipAttr, resource);
                object trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, relationshipIds.ToArray() );
                LoadInverseRelationships(trackedRelationshipValue, relationshipAttr);
                relationshipAttr.SetValue(resource, trackedRelationshipValue, _resourceFactory);
            }

            _dbContext.Set<TResource>().Add(resource);
            await _dbContext.SaveChangesAsync();

            FlushFromCache(resource);

            // this ensures relationships get reloaded from the database if they have
            // been requested. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
            DetachRelationships(resource);
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
        private void LoadInverseRelationships(object trackedRelationshipValue, RelationshipAttribute relationshipAttr)
        {
            if (relationshipAttr.InverseNavigation == null || trackedRelationshipValue == null) return;
            if (relationshipAttr is HasOneAttribute hasOneAttr)
            {
                var relationEntry = _dbContext.Entry((IIdentifiable)trackedRelationshipValue);
                if (IsHasOneRelationship(hasOneAttr.InverseNavigation, trackedRelationshipValue.GetType()))
                    relationEntry.Reference(hasOneAttr.InverseNavigation).Load();
                else
                    relationEntry.Collection(hasOneAttr.InverseNavigation).Load();
            }
            else if (relationshipAttr is HasManyAttribute hasManyAttr && !(relationshipAttr is HasManyThroughAttribute))
            {
                foreach (IIdentifiable relationshipValue in (IEnumerable)trackedRelationshipValue)
                    _dbContext.Entry(relationshipValue).Reference(hasManyAttr.InverseNavigation).Load();
            }
        }

        private bool IsHasOneRelationship(string internalRelationshipName, Type type)
        {
            var relationshipAttr = _resourceGraph.GetRelationships(type).FirstOrDefault(r => r.Property.Name == internalRelationshipName);
            if (relationshipAttr != null)
            {
                if (relationshipAttr is HasOneAttribute)
                    return true;

                return false;
            }
            // relationshipAttr is null when we don't put a [RelationshipAttribute] on the inverse navigation property.
            // In this case we use reflection to figure out what kind of relationship is pointing back.
            return !TypeHelper.IsOrImplementsInterface(type.GetProperty(internalRelationshipName).PropertyType, typeof(IEnumerable));
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

            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                // loads databasePerson.todoItems. Required for complete replacements
                LoadCurrentRelationships(databaseResource, relationshipAttr);
                
                // trackedRelationshipValue is either equal to updatedPerson.todoItems,
                // or replaced with the same set (same ids) of todoItems from the EF Core change tracker,
                // which is the case if they were already tracked
                var relationshipIds = GetRelationshipIds(relationshipAttr, requestResource);
                object trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, relationshipIds);
                
                // loads into the db context any persons currently related
                // to the todoItems in trackedRelationshipValue
                LoadInverseRelationships(trackedRelationshipValue, relationshipAttr);
                
                // assigns the updated relationship to the database resource
                //AssignRelationshipValue(databaseResource, trackedRelationshipValue, relationshipAttr);
                relationshipAttr.SetValue(databaseResource, trackedRelationshipValue, _resourceFactory);
            }

            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Responsible for getting the relationship value for a given relationship 
        /// attribute of a given resource. It ensures that the relationship value 
        /// that it returns is attached to the database without reattaching duplicates instances 
        /// to the change tracker. It does so by checking if there already are
        /// instances of the to-be-attached entities in the change tracker.
        /// </summary>
        private string[] GetRelationshipIds(RelationshipAttribute relationship, TResource requestResource)
        {
            if (relationship is HasOneAttribute hasOneAttr)
            {
                var relationshipValue = (IIdentifiable) hasOneAttr.GetValue(requestResource);

                if (relationshipValue == null)
                {
                    return new string[0];
                }

                return new[] { relationshipValue.StringId };
            }
            else
            {
                var hasManyAttr = (HasManyAttribute)relationship;
                var relationshipValuesCollection = (IEnumerable<IIdentifiable>)hasManyAttr.GetValue(requestResource);

                return relationshipValuesCollection.Select(i => i.StringId).ToArray();
            }
        }
        
        private object GetTrackedRelationshipValue(RelationshipAttribute relationship, params string[] relationshipIds)
        {
            object trackedRelationshipValue;
            var entityType = relationship.RightType;
            
            if (relationship is HasOneAttribute)
            {
                if (!relationshipIds.Any())
                {
                    return null;
                }
                
                var id = relationshipIds.Single();
                trackedRelationshipValue = GetTrackedOrNewlyAttachedEntity(entityType, id);
            }
            else
            {
                var amountOfValues = relationshipIds.Count();
                var collection = new object[amountOfValues];

                for (int i = 0; i < amountOfValues; i++)
                {
                    var elementOfRelationshipValue = GetTrackedOrNewlyAttachedEntity(entityType, relationshipIds[i]);
                    collection[i] = Convert.ChangeType(elementOfRelationshipValue, entityType);
                }

                trackedRelationshipValue = TypeHelper.CopyToTypedCollection(collection, relationship.Property.PropertyType);
            }


            return trackedRelationshipValue;
        }
        
        private IIdentifiable GetTrackedOrNewlyAttachedEntity(Type resourceType, string id)
        {
            var trackedEntity = _dbContext.GetTrackedEntity(resourceType, id);
            if (trackedEntity == null)
            {
                // the relationship pointer is new to EF Core, but we are sure
                // it exists in the database, so we attach it. In this case, as per
                // the json:api spec, we can also safely assume that no fields of 
                // this resource were updated. Note that if it was already tracked, reattaching it
                // will throw an error when calling dbContext.SaveAsync();
                trackedEntity = (IIdentifiable) _resourceFactory.CreateInstance(resourceType);
                trackedEntity.StringId = id;
                _dbContext.Entry(trackedEntity).State = EntityState.Unchanged;
            }

            return trackedEntity;
        }
        
        /// <inheritdoc />
        public async Task SetRelationshipsAsync(TResource parent, RelationshipAttribute relationship, IReadOnlyCollection<string> relationshipIds)
        {
            _traceWriter.LogMethodStart(new {parent, relationship, relationshipIds});
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (relationshipIds == null) throw new ArgumentNullException(nameof(relationshipIds));

            LoadCurrentRelationships(parent, relationship);
            object trackedRelationshipValue = GetTrackedRelationshipValue(relationship, relationshipIds.ToArray());
            LoadInverseRelationships(trackedRelationshipValue, relationship);
            relationship.SetValue(parent, trackedRelationshipValue, _resourceFactory);

            await _dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            var resourceToDelete = _resourceFactory.CreateInstance<TResource>();
            resourceToDelete.Id = id;

            var resourceFromCache = _dbContext.GetTrackedEntity(resourceToDelete);
            if (resourceFromCache != null)
            {
                resourceToDelete = resourceFromCache;
            }
            else
            {
                _dbContext.Attach(resourceToDelete);
            }

            _dbContext.Remove(resourceToDelete);

            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
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
        protected void LoadCurrentRelationships(TResource oldResource, RelationshipAttribute relationshipAttribute)
        {
            if (oldResource == null) throw new ArgumentNullException(nameof(oldResource));
            if (relationshipAttribute == null) throw new ArgumentNullException(nameof(relationshipAttribute));

            if (relationshipAttribute is HasManyThroughAttribute throughAttribute)
            {
                _dbContext.Entry(oldResource).Collection(throughAttribute.ThroughProperty.Name).Load();
            }
            else if (relationshipAttribute is HasManyAttribute hasManyAttribute)
            {
                _dbContext.Entry(oldResource).Collection(hasManyAttribute.Property.Name).Load();
            }
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
            IResourceAccessor resourceAccessor,
            ILoggerFactory loggerFactory)
            : base(targetedFields, contextResolver, resourceGraph, resourceFactory, constraintProviders, resourceAccessor, loggerFactory) 
        { }
    }
}
