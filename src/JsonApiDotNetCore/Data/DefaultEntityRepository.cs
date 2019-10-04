using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace JsonApiDotNetCore.Data
{
    /// <summary>
    /// Provides a default repository implementation and is responsible for
    /// abstracting any EF Core APIs away from the service layer.
    /// </summary>
    public class DefaultEntityRepository<TEntity, TId>
        : IEntityRepository<TEntity, TId>,
        IEntityFrameworkRepository<TEntity>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly ICurrentRequest _currentRequest;
        private readonly ITargetedFields _targetedFields;
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly ILogger _logger;
        private readonly IResourceGraph _resourceGraph;
        private readonly IGenericProcessorFactory _genericProcessorFactory;
        private readonly ResourceDefinition<TEntity> _resourceDefinition;

        public DefaultEntityRepository(
            ITargetedFields updatedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IGenericProcessorFactory genericProcessorFactory,
            ResourceDefinition<TEntity> resourceDefinition = null)
            : this(null, updatedFields, contextResolver, resourceGraph, genericProcessorFactory, resourceDefinition)
        { }

        public DefaultEntityRepository(
            ILoggerFactory loggerFactory,
            ITargetedFields updatedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IGenericProcessorFactory genericProcessorFactory,
            ResourceDefinition<TEntity> resourceDefinition = null)
        {
            _logger = loggerFactory.CreateLogger<DefaultEntityRepository<TEntity, TId>>();
            _targetedFields = updatedFields;
            _resourceGraph = resourceGraph;
            _genericProcessorFactory = genericProcessorFactory;
            _context = contextResolver.GetContext();
            _dbSet = _context.Set<TEntity>();
            _resourceDefinition = resourceDefinition;
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Get() => _dbSet;

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
            return entities.Filter(new AttrFilterQuery(_currentRequest.GetRequestResource(), _resourceGraph, filterQuery));
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries)
        {
            if (sortQueries != null && sortQueries.Count > 0)
                return entities.Sort(_currentRequest.GetRequestResource(), _resourceGraph, sortQueries);

            if (_resourceDefinition != null)
            {
                var defaultSortOrder = _resourceDefinition.DefaultSort();
                if (defaultSortOrder != null && defaultSortOrder.Count > 0)
                {
                    foreach (var sortProp in defaultSortOrder)
                    {
                        // this is dumb...add an overload, don't allocate for no reason
                        entities.Sort(_currentRequest.GetRequestResource(), _resourceGraph, new SortQuery(sortProp.Item2, sortProp.Item1.PublicAttributeName));
                    }
                }
            }
            return entities;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TId id)
        {
            return await Select(Get(), _currentRequest.QuerySet?.Fields).SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            _logger?.LogDebug($"[JADN] GetAndIncludeAsync({id}, {relationshipName})");
            var includedSet = Include(Select(Get(), _currentRequest.QuerySet?.Fields), relationshipName);
            var result = await includedSet.SingleOrDefaultAsync(e => e.Id.Equals(id));
            return result;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                var trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, entity, out bool wasAlreadyTracked);
                LoadInverseRelationships(trackedRelationshipValue, relationshipAttr);
                if (wasAlreadyTracked)
                {
                    /// We only need to reassign the relationship value to the to-be-added
                    /// entity when we're using a different instance (because this different one
                    /// was already tracked) than the one assigned to the to-be-created entity.
                    AssignRelationshipValue(entity, trackedRelationshipValue, relationshipAttr);
                }
                else if (relationshipAttr is HasManyThroughAttribute throughAttr)
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

        /// <summary>
        /// Loads the inverse relationships to prevent foreign key constraints from being violated
        /// to support implicit removes, see https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/502.
        /// <remark>
        /// Consider the following example: 
        /// person.todoItems = [t1,t2] is updated to [t3, t4]. If t3, and/or t4 was
        /// already related to a other person, and these persons are NOT loaded in to the 
        /// db context, then the query may cause a foreign key constraint. Loading
        /// these "inverse relationships" into the DB context ensures EF core to take
        /// this into account.
        /// </remark>
        /// </summary>
        private void LoadInverseRelationships(object trackedRelationshipValue, RelationshipAttribute relationshipAttr)
        {
            if (relationshipAttr.InverseNavigation == null || trackedRelationshipValue == null) return;
            if (relationshipAttr is HasOneAttribute hasOneAttr)
            {
                var relationEntry = _context.Entry((IIdentifiable)trackedRelationshipValue);
                if (IsHasOneRelationship(hasOneAttr.InverseNavigation, trackedRelationshipValue.GetType()))
                {
                    relationEntry.Reference(hasOneAttr.InverseNavigation).Load();
                }
                else
                {
                    relationEntry.Collection(hasOneAttr.InverseNavigation).Load();
                }
            }
            else if (relationshipAttr is HasManyAttribute hasManyAttr && !(relationshipAttr is HasManyThroughAttribute))
            {
                foreach (IIdentifiable relationshipValue in (IList)trackedRelationshipValue)
                {
                    _context.Entry(relationshipValue).Reference(hasManyAttr.InverseNavigation).Load();
                }
            }
        }

        private bool IsHasOneRelationship(string internalRelationshipName, Type type)
        {
            var relationshipAttr = _resourceGraph.GetContextEntity(type).Relationships.SingleOrDefault(r => r.InternalRelationshipName == internalRelationshipName);
            if (relationshipAttr != null)
            {
                if (relationshipAttr is HasOneAttribute) return true;
                return false;
            }
            else
            {
                // relationshipAttr is null when we don't put a [RelationshipAttribute] on the inverse navigation property.
                // In this case we use relfection to figure out what kind of relationship is pointing back.
                return !(type.GetProperty(internalRelationshipName).PropertyType.Inherits(typeof(IEnumerable)));
            }
        }


        /// <inheritdoc />
        public void DetachRelationshipPointers(TEntity entity)
        {

            foreach (var relationshipAttr in _targetedFields.Relationships)
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

        [Obsolete("Use overload UpdateAsync(TEntity updatedEntity): providing parameter ID does no longer add anything relevant")]
        public virtual async Task<TEntity> UpdateAsync(TId id, TEntity updatedEntity)
        {
            return await UpdateAsync(updatedEntity);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TEntity updatedEntity)
        {
            var databaseEntity = await GetAsync(updatedEntity.Id);
            if (databaseEntity == null)
                return null;

            foreach (var attr in _targetedFields.Attributes)
                attr.SetValue(databaseEntity, attr.GetValue(updatedEntity));

            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                /// loads databasePerson.todoItems
                LoadCurrentRelationships(databaseEntity, relationshipAttr);
                /// trackedRelationshipValue is either equal to updatedPerson.todoItems 
                /// or replaced with the same set of todoItems from the EF Core change tracker, 
                /// if they were already tracked
                object trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, updatedEntity, out bool wasAlreadyTracked);
                /// loads into the db context any persons currently related 
                /// to the todoItems in trackedRelationshipValue
                LoadInverseRelationships(trackedRelationshipValue, relationshipAttr);
                /// assigns the updated relationship to the database entity
                AssignRelationshipValue(databaseEntity, trackedRelationshipValue, relationshipAttr);
            }

            await _context.SaveChangesAsync();
            return databaseEntity;
        }


        /// <summary>
        /// Responsible for getting the relationship value for a given relationship 
        /// attribute of a given entity. It ensures that the relationship value 
        /// that it returns is attached to the database without reattaching duplicates instances 
        /// to the change tracker. It does so by checking if there already are
        /// instances of the to-be-attached entities in the change tracker.
        /// </summary>
        private object GetTrackedRelationshipValue(RelationshipAttribute relationshipAttr, TEntity entity, out bool wasAlreadyAttached)
        {
            wasAlreadyAttached = false;
            if (relationshipAttr is HasOneAttribute hasOneAttr)
            {
                /// This adds support for resource-entity separation in the case of one-to-one. 
                var relationshipValue = GetEntityResourceSeparationValue(entity, hasOneAttr) ?? (IIdentifiable)hasOneAttr.GetValue(entity);
                if (relationshipValue == null)
                    return null;
                return GetTrackedHasOneRelationshipValue(relationshipValue, hasOneAttr, ref wasAlreadyAttached);
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

        // helper method used in GetTrackedRelationshipValue. See comments there.
        private IList GetTrackedManyRelationshipValue(IEnumerable<IIdentifiable> relationshipValueList, RelationshipAttribute relationshipAttr, ref bool wasAlreadyAttached)
        {
            if (relationshipValueList == null) return null;
            bool _wasAlreadyAttached = false;
            /// if we're not using entity resource separation, we can just read off the related type
            /// from the RelationshipAttribute. If we DO use separation, RelationshipAttribute.DependentType
            /// will point to the Resource, not the Entity, which is not the one we need here.
            bool entityResourceSeparation = relationshipAttr.EntityPropertyName != null;
            Type entityType = entityResourceSeparation ? null : relationshipAttr.DependentType;
            var trackedPointerCollection = relationshipValueList.Select(pointer =>
            {
                /// todo: we can't just use relationshipAttr.DependentType because
                /// this will point to the Resource type in the case of entity resource
                /// separation. We should consider to store entity type on 
                /// the relationship attribute too.
                entityType = entityType ?? pointer.GetType();
                var tracked = AttachOrGetTracked(pointer);
                if (tracked != null) _wasAlreadyAttached = true;
                return Convert.ChangeType(tracked ?? pointer, entityType);
            }).ToList().Cast(entityType);
            if (_wasAlreadyAttached) wasAlreadyAttached = true;
            return (IList)trackedPointerCollection;
        }

        // helper method used in GetTrackedRelationshipValue. See comments there.
        private IIdentifiable GetTrackedHasOneRelationshipValue(IIdentifiable relationshipValue, HasOneAttribute hasOneAttr, ref bool wasAlreadyAttached)
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

        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities, params RelationshipAttribute[] inclusionChain)
        {
            string internalRelationshipPath = null;
            foreach (var relationship in inclusionChain)
                internalRelationshipPath = (internalRelationshipPath == null)
                    ? relationship.RelationshipPath
                    : $"{internalRelationshipPath}.{relationship.RelationshipPath}";

            return entities.Include(internalRelationshipPath);
        }

        /// <inheritdoc />
        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName)
        {
            if (string.IsNullOrWhiteSpace(relationshipName)) throw new JsonApiException(400, "Include parameter must not be empty if provided");

            var relationshipChain = relationshipName.Split('.');

            // variables mutated in recursive loop
            // TODO: make recursive method
            string internalRelationshipPath = null;
            var entity = _currentRequest.GetRequestResource();
            for (var i = 0; i < relationshipChain.Length; i++)
            {
                var requestedRelationship = relationshipChain[i];
                var relationship = entity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == requestedRelationship);

                internalRelationshipPath = (internalRelationshipPath == null)
                    ? relationship.RelationshipPath
                    : $"{internalRelationshipPath}.{relationship.RelationshipPath}";

                if (i < relationshipChain.Length)
                    entity = _resourceGraph.GetContextEntity(relationship.Type);
            }

            return entities.Include(internalRelationshipPath);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber)
        {
            if (pageNumber >= 0)
            {
                // the IQueryable returned from the hook executor is sometimes consumed here.
                // In this case, it does not support .ToListAsync(), so we use the method below.
                return await this.ToListAsync(entities.PageForward(pageSize, pageNumber));
            }

            // since EntityFramework does not support IQueryable.Reverse(), we need to know the number of queried entities
            int numberOfEntities = await this.CountAsync(entities);

            // may be negative
            int virtualFirstIndex = numberOfEntities - pageSize * Math.Abs(pageNumber);
            int numberOfElementsInPage = Math.Min(pageSize, virtualFirstIndex + pageSize);

            return await ToListAsync(entities
                    .Skip(virtualFirstIndex)
                    .Take(numberOfElementsInPage));
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
        /// attach the current database values of the relationship to the dbcontext, else 
        /// it will not perform a complete-replace which is required for 
        /// one-to-many and many-to-many.
        /// <para />
        /// For example: a person `p1` has 2 todoitems: `t1` and `t2`.
        /// If we want to update this todoitem set to `t3` and `t4`, simply assigning
        /// `p1.todoItems = [t3, t4]` will result in EF Core adding them to the set,
        /// resulting in `[t1 ... t4]`. Instead, we should first include `[t1, t2]`,
        /// after which the reassignment  `p1.todoItems = [t3, t4]` will actually 
        /// make EF Core perform a complete replace. This method does the loading of `[t1, t2]`.
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
        /// Assigns the <paramref name="relationshipValue"/> to <paramref name="targetEntity"/>
        /// </summary>
        private void AssignRelationshipValue(TEntity targetEntity, object relationshipValue, RelationshipAttribute relationshipAttribute)
        {
            if (relationshipAttribute is HasManyThroughAttribute throughAttribute)
            {
                // todo: this logic should be put in the HasManyThrough attribute
                AssignHasManyThrough(targetEntity, throughAttribute, (IList)relationshipValue);
            }
            else
            {
                relationshipAttribute.SetValue(targetEntity, relationshipValue);
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
        private IIdentifiable GetEntityResourceSeparationValue(TEntity entity, HasOneAttribute attribute)
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
        private IEnumerable<IIdentifiable> GetEntityResourceSeparationValue(TEntity entity, HasManyAttribute attribute)
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
        private IIdentifiable AttachOrGetTracked(IIdentifiable relationshipValue)
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

    /// <inheritdoc />
    public class DefaultEntityRepository<TEntity>
        : DefaultEntityRepository<TEntity, int>,
        IEntityRepository<TEntity>
        where TEntity : class, IIdentifiable<int>
    {

        public DefaultEntityRepository(
            ITargetedFields updatedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IGenericProcessorFactory genericProcessorFactory,
            ResourceDefinition<TEntity> resourceDefinition = null) :
            base(updatedFields, contextResolver, resourceGraph,
                genericProcessorFactory, resourceDefinition)
        {
        }

        public DefaultEntityRepository(ILoggerFactory loggerFactory,
                                       ITargetedFields updatedFields,
                                       IDbContextResolver contextResolver,
                                       IResourceGraph resourceGraph,
                                       IGenericProcessorFactory genericProcessorFactory,
                                       ResourceDefinition<TEntity> resourceDefinition = null) :
            base(loggerFactory, updatedFields, contextResolver, resourceGraph,
                genericProcessorFactory, resourceDefinition)
        {
        }
    }
}
