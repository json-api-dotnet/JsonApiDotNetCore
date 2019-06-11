using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace JsonApiDotNetCore.Data
{
    /// <inheritdoc />
    public class DefaultEntityRepository<TEntity>
        : DefaultEntityRepository<TEntity, int>,
        IEntityRepository<TEntity>
        where TEntity : class, IIdentifiable<int>
    {
        public DefaultEntityRepository(
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver,
            ResourceDefinition<TEntity> resourceDefinition = null)
        : base(jsonApiContext, contextResolver, resourceDefinition)
        { }

        public DefaultEntityRepository(
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver,
            ResourceDefinition<TEntity> resourceDefinition = null)
        : base(loggerFactory, jsonApiContext, contextResolver, resourceDefinition)
        { }
    }

    /// <summary>
    /// Provides a default repository implementation and is responsible for
    /// abstracting any EF Core APIs away from the service layer.
    /// </summary>
    public class DefaultEntityRepository<TEntity, TId>
        : IEntityRepository<TEntity, TId>,
        IEntityFrameworkRepository<TEntity>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly ILogger _logger;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IGenericProcessorFactory _genericProcessorFactory;
        private readonly ResourceDefinition<TEntity> _resourceDefinition;
        public DefaultEntityRepository(
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver,
            ResourceDefinition<TEntity> resourceDefinition = null)
        {
            _context = contextResolver.GetContext();
            _dbSet = contextResolver.GetDbSet<TEntity>();
            _jsonApiContext = jsonApiContext;
            _genericProcessorFactory = _jsonApiContext.GenericProcessorFactory;
            _resourceDefinition = resourceDefinition;
        }

        public DefaultEntityRepository(
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver,
            ResourceDefinition<TEntity> resourceDefinition = null)
        {
            _context = contextResolver.GetContext();
            _dbSet = contextResolver.GetDbSet<TEntity>();
            _jsonApiContext = jsonApiContext;
            _logger = loggerFactory.CreateLogger<DefaultEntityRepository<TEntity, TId>>();
            _genericProcessorFactory = _jsonApiContext.GenericProcessorFactory;
            _resourceDefinition = resourceDefinition;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Get()
            => _dbSet;

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Select(IQueryable<TEntity> entities, List<string> fields)
        {
            if (fields?.Count > 0)
                return entities.Select(fields);

            return entities;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery)
        {
            if (_resourceDefinition != null)
            {
                var defaultQueryFilters = _resourceDefinition.GetQueryFilters();
                if (defaultQueryFilters != null && defaultQueryFilters.TryGetValue(filterQuery.Attribute, out var defaultQueryFilter) == true)
                {
                    return defaultQueryFilter(entities, filterQuery);
                }
            }
            return entities.Filter(_jsonApiContext, filterQuery);
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries)
        {
            if (sortQueries != null && sortQueries.Count > 0)
                return entities.Sort(_jsonApiContext, sortQueries);

            if (_resourceDefinition != null)
            {
                var defaultSortOrder = _resourceDefinition.DefaultSort();
                if (defaultSortOrder != null && defaultSortOrder.Count > 0)
                {
                    foreach (var sortProp in defaultSortOrder)
                    {
                        // this is dumb...add an overload, don't allocate for no reason
                        entities.Sort(_jsonApiContext, new SortQuery(sortProp.Item2, sortProp.Item1.PublicAttributeName));
                    }
                }
            }
            return entities;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TId id)
        {
            return await Select(Get(), _jsonApiContext.QuerySet?.Fields).SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            _logger?.LogDebug($"[JADN] GetAndIncludeAsync({id}, {relationshipName})");

            var includedSet = Include(Select(Get(), _jsonApiContext.QuerySet?.Fields), relationshipName);
            var result = await includedSet.SingleOrDefaultAsync(e => e.Id.Equals(id));

            return result;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            foreach (var relationshipAttr in _jsonApiContext.RelationshipsToUpdate?.Keys)
            {
                var trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, entity, out bool wasAlreadyTracked);
                // LoadInverseRelationships(trackedRelationshipValue, relationshipAttribute)
                if (wasAlreadyTracked)
                {
                    /// We only need to reassign the relationship value to the to-be-added
                    /// entity when we're using a different instance (because this different one
                    /// was already tracked) than the one assigned to the to-be-created entity.
                    AssignRelationshipValue(entity, trackedRelationshipValue, relationshipAttr);
                } else if (relationshipAttr is HasManyThroughAttribute throughAttr)
                {
                    /// even if we don't have to reassign anything because of already tracked 
                    /// entities, we still need to assign the "through" entities in the case of many-to-many.
                    AssignHasManyThrough(entity, throughAttr, (IList)trackedRelationshipValue);
                }
            }
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }


        /// <inheritdoc />
        public void DetachRelationshipPointers(TEntity entity)
        {

            foreach (var relationshipAttr in _jsonApiContext.RelationshipsToUpdate.Keys)
            {
                if (relationshipAttr is HasOneAttribute hasOneAttr) 
                {
                    var relationshipValue = GetEntityResourceSeparationValue(entity, hasOneAttr) ?? (IIdentifiable)hasOneAttr.GetValue(entity);
                    if (relationshipValue == null) continue;
                    _context.Entry(relationshipValue).State = EntityState.Detached;

                }
                else 
                {
                    IEnumerable<IIdentifiable> relationshipValueList = (IEnumerable<IIdentifiable>)relationshipAttr.GetValue(entity);
                    /// This adds support for resource-entity separation in the case of one-to-many. 
                    /// todo: currently there is no support for many to many relations.
                    if (relationshipAttr is HasManyAttribute hasMany)
                        relationshipValueList = GetEntityResourceSeparationValue(entity, hasMany) ?? relationshipValueList;
                    if (relationshipValueList == null) continue;
                    foreach (var pointer in relationshipValueList)
                    {
                        _context.Entry(pointer).State = EntityState.Detached;
                    }
                    /// detaching has many relationships is not sufficient to 
                    /// trigger a full reload of relationships: the navigation 
                    /// property actually needs to be nulled out, otherwise
                    /// EF will still add duplicate instances to the collection
                    relationshipAttr.SetValue(entity, null);
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TId id, TEntity updatedEntity)
        {
            /// WHY is parameter "entity" even passed along to this method??
            /// It does nothing!

            var oldEntity = await GetAsync(id);

            if (oldEntity == null)
                return null;

            foreach (var attr in _jsonApiContext.AttributesToUpdate.Keys)
                attr.SetValue(oldEntity, attr.GetValue(updatedEntity));

            foreach (var relationshipAttr in _jsonApiContext.RelationshipsToUpdate?.Keys)
            {
                LoadCurrentRelationships(oldEntity, relationshipAttr);
                var trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, updatedEntity, out bool wasAlreadyTracked);
                // LoadInverseRelationships(trackedRelationshipValue, relationshipAttribute)
                AssignRelationshipValue(oldEntity, trackedRelationshipValue, relationshipAttr);
            }
            await _context.SaveChangesAsync();
            return oldEntity;
        }


        /// <summary>
        /// Responsible for getting the relationship value for a given relationship 
        /// attribute of a given entity. It ensures that the relationship value 
        /// that it returns is attached to the database without reattaching duplicates instances 
        /// to the change tracker.
        /// </summary>
        private object GetTrackedRelationshipValue(RelationshipAttribute relationshipAttr, TEntity entity, out bool wasAlreadyAttached)
        {
            wasAlreadyAttached = false;
            if (relationshipAttr is HasOneAttribute hasOneAttribute)
            {
                /// This adds support for resource-entity separation in the case of one-to-one. 
                var relationshipValue = GetEntityResourceSeparationValue(entity, hasOneAttribute) ?? (IIdentifiable)hasOneAttribute.GetValue(entity);
                if (relationshipValue == null) 
                        return null;
                return GetTrackedHasOneRelationshipValue(relationshipValue, hasOneAttribute, ref wasAlreadyAttached);
            }
            else
            {
                IEnumerable<IIdentifiable> relationshipValueList = (IEnumerable<IIdentifiable>)relationshipAttr.GetValue(entity);
                /// This adds support for resource-entity separation in the case of one-to-many. 
                /// todo: currently there is no support for many to many relations.
                if (relationshipAttr is HasManyAttribute hasMany) 
                        relationshipValueList = GetEntityResourceSeparationValue(entity, hasMany) ?? relationshipValueList;
                if (relationshipValueList == null) return null;
                return GetTrackedManyRelationshipValue(relationshipValueList, relationshipAttr, ref wasAlreadyAttached);
            }
        }

        private IList GetTrackedManyRelationshipValue(IEnumerable<IIdentifiable> relationshipValueList, RelationshipAttribute relationshipAttr, ref bool wasAlreadyAttached)
        {
            if (relationshipValueList == null) return null;
            bool _wasAlreadyAttached = false;
            var trackedPointerCollection = relationshipValueList.Select(pointer =>
            {
                var tracked = AttachOrGetTracked(pointer);
                if (tracked != null) _wasAlreadyAttached = true;
                return Convert.ChangeType(tracked ?? pointer, relationshipAttr.Type);
            }).ToList().Cast(relationshipAttr.Type);
            if (_wasAlreadyAttached) wasAlreadyAttached = true;
            return (IList)trackedPointerCollection;
        }

        private IIdentifiable GetTrackedHasOneRelationshipValue(IIdentifiable relationshipValue, HasOneAttribute hasOneAttribute, ref bool wasAlreadyAttached)
        {

            var tracked = AttachOrGetTracked(relationshipValue);
            if (tracked != null) wasAlreadyAttached = true;
            return tracked ?? relationshipValue;
        }

        /// <inheritdoc />
        public async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            // TODO: it would be better to let this be determined within the relationship attribute...
            // need to think about the right way to do that since HasMany doesn't need to think about this
            // and setting the HasManyThrough.Type to the join type (ArticleTag instead of Tag) for this changes the semantics
            // of the property...
            var typeToUpdate = (relationship is HasManyThroughAttribute hasManyThrough)
                ? hasManyThrough.ThroughType
                : relationship.Type;

            var genericProcessor = _genericProcessorFactory.GetProcessor<IGenericProcessor>(typeof(GenericProcessor<>), typeToUpdate);
            await genericProcessor.UpdateRelationshipsAsync(parent, relationship, relationshipIds);
        }


        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = await GetAsync(id);
            if (entity == null) return false;
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName)
        {
            if (string.IsNullOrWhiteSpace(relationshipName)) throw new JsonApiException(400, "Include parameter must not be empty if provided");

            var relationshipChain = relationshipName.Split('.');

            // variables mutated in recursive loop
            // TODO: make recursive method
            string internalRelationshipPath = null;
            var entity = _jsonApiContext.RequestEntity;
            for (var i = 0; i < relationshipChain.Length; i++)
            {
                var requestedRelationship = relationshipChain[i];
                var relationship = entity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == requestedRelationship);
                if (relationship == null)
                {
                    throw new JsonApiException(400, $"Invalid relationship {requestedRelationship} on {entity.EntityName}",
                        $"{entity.EntityName} does not have a relationship named {requestedRelationship}");
                }

                if (relationship.CanInclude == false)
                {
                    throw new JsonApiException(400, $"Including the relationship {requestedRelationship} on {entity.EntityName} is not allowed");
                }

                internalRelationshipPath = (internalRelationshipPath == null)
                    ? relationship.RelationshipPath
                    : $"{internalRelationshipPath}.{relationship.RelationshipPath}";

                if (i < relationshipChain.Length)
                    entity = _jsonApiContext.ResourceGraph.GetContextEntity(relationship.Type);
            }

            return entities.Include(internalRelationshipPath);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber)
        {
            if (pageNumber >= 0)
            {
                return await entities.PageForward(pageSize, pageNumber).ToListAsync();
            }

            // since EntityFramework does not support IQueryable.Reverse(), we need to know the number of queried entities
            int numberOfEntities = await this.CountAsync(entities);

            // may be negative
            int virtualFirstIndex = numberOfEntities - pageSize * Math.Abs(pageNumber);
            int numberOfElementsInPage = Math.Min(pageSize, virtualFirstIndex + pageSize);

            return await entities
                    .Skip(virtualFirstIndex)
                    .Take(numberOfElementsInPage)
                    .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(IQueryable<TEntity> entities)
        {
            return (entities is IAsyncEnumerable<TEntity>)
                 ? await entities.CountAsync()
                 : entities.Count();
        }

        /// <inheritdoc />
        public async Task<TEntity> FirstOrDefaultAsync(IQueryable<TEntity> entities)
        {
            return (entities is IAsyncEnumerable<TEntity>)
               ? await entities.FirstOrDefaultAsync()
               : entities.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TEntity>> ToListAsync(IQueryable<TEntity> entities)
        {
            return (entities is IAsyncEnumerable<TEntity>)
                ? await entities.ToListAsync()
                : entities.ToList();
        }


        /// <summary>
        /// Before assigning new relationship values (UpdateAsync), we need to
        /// attach the current relationship state to the dbcontext, else 
        /// it will not perform a complete-replace which is required for 
        /// one-to-many and many-to-many.
        /// </summary>
        protected void LoadCurrentRelationships(TEntity oldEntity, RelationshipAttribute relationshipAttribute)
        {
            if (relationshipAttribute is HasManyThroughAttribute throughAttribute)
            {
                _context.Entry(oldEntity).Collection(throughAttribute.InternalThroughName).Load();

            }
            else if (relationshipAttribute is HasManyAttribute hasManyAttribute)
            {
                _context.Entry(oldEntity).Collection(hasManyAttribute.InternalRelationshipName).Load();
            }
        }

        /// <summary>
        /// assigns relationships that were set in the request to the target entity of the request
        /// todo: partially remove dependency on IJsonApiContext here: it is fine to
        /// retrieve from the context WHICH relationships to update, but the actual
        /// values should not come from the context.
        /// </summary>
        protected void AssignRelationshipValue(TEntity oldEntity, object relationshipValue, RelationshipAttribute relationshipAttribute)
        {
            if (relationshipAttribute is HasManyThroughAttribute throughAttribute)
            {
                // todo: this logic should be put in the HasManyThrough attribute
                AssignHasManyThrough(oldEntity, throughAttribute, (IList)relationshipValue);
            }
            else
            {
                relationshipAttribute.SetValue(oldEntity, relationshipValue);
            }
        }

        /// <summary>
        /// The relationshipValue parameter contains the dependent side of the relationship (Tags).
        /// We can't directly add them to the principal entity (Article): we need to 
        /// use the join table (ArticleTags). This methods assigns the relationship value to entity
        /// by taking care of that
        /// </summary>
        private void AssignHasManyThrough(TEntity entity, HasManyThroughAttribute hasManyThrough, IList relationshipValue)
        {
            var pointers = relationshipValue.Cast<IIdentifiable>();
            var throughRelationshipCollection = Activator.CreateInstance(hasManyThrough.ThroughProperty.PropertyType) as IList;
            hasManyThrough.ThroughProperty.SetValue(entity, throughRelationshipCollection);

            foreach (var pointer in pointers)
            {
                var throughInstance = Activator.CreateInstance(hasManyThrough.ThroughType);
                hasManyThrough.LeftProperty.SetValue(throughInstance, entity);
                hasManyThrough.RightProperty.SetValue(throughInstance, pointer);
                throughRelationshipCollection.Add(throughInstance);
            }
        }

        /// <summary>
        /// A helper method that gets the relationship value in the case of 
        /// entity resource separation.
        /// </summary>
        IIdentifiable GetEntityResourceSeparationValue(TEntity entity, HasOneAttribute attribute)
        {
            if (attribute.EntityPropertyName == null)
            {
                return null;
            }
            return (IIdentifiable)entity.GetType().GetProperty(attribute.EntityPropertyName)?.GetValue(entity);
        }

        /// <summary>
        /// A helper method that gets the relationship value in the case of 
        /// entity resource separation.
        /// </summary>
        IEnumerable<IIdentifiable> GetEntityResourceSeparationValue(TEntity entity, HasManyAttribute attribute)
        {
            if (attribute.EntityPropertyName == null)
            {
                return null;
            }
            return ((IEnumerable)(entity.GetType().GetProperty(attribute.EntityPropertyName)?.GetValue(entity))).Cast<IIdentifiable>();
        }

        /// <summary>
        /// Given a iidentifiable relationshipvalue, verify if an entity of the underlying 
        /// type with the same ID is already attached to the dbContext, and if so, return it.
        /// If not, attach the relationship value to the dbContext.
        /// 
        /// useful article: https://stackoverflow.com/questions/30987806/dbset-attachentity-vs-dbcontext-entryentity-state-entitystate-modified
        /// </summary>
        IIdentifiable AttachOrGetTracked(IIdentifiable relationshipValue)
        {
            var trackedEntity = _context.GetTrackedEntity(relationshipValue);

            if (trackedEntity != null)
            {
                /// there already was an instance of this type and ID tracked
                /// by EF Core. Reattaching will produce a conflict, so from now on we 
                /// will use the already attached instance instead. This entry might
                /// contain updated fields as a result of business logic elsewhere in the application
                return trackedEntity;
            }

            /// the relationship pointer is new to EF Core, but we are sure
            /// it exists in the database, so we attach it. In this case, as per
            /// the json:api spec, we can also safely assume that no fields of 
            /// this entity were updated.
            _context.Entry(relationshipValue).State = EntityState.Unchanged;
            return null;
        }
    }
}
