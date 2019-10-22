using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Internal.Query;
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
    public class DefaultResourceRepository<TResource, TId> : IResourceRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly ITargetedFields _targetedFields;
        private readonly DbContext _context;
        private readonly DbSet<TResource> _dbSet;
        private readonly IResourceGraph _resourceGraph;
        private readonly IGenericProcessorFactory _genericProcessorFactory;

        public DefaultResourceRepository(
            ITargetedFields targetedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IGenericProcessorFactory genericProcessorFactory)
            : this(targetedFields, contextResolver, resourceGraph, genericProcessorFactory, null)
        { }

        public DefaultResourceRepository(
            ITargetedFields targetedFields,
            IDbContextResolver contextResolver,
            IResourceGraph resourceGraph,
            IGenericProcessorFactory genericProcessorFactory,
            ILoggerFactory loggerFactory = null)
        {
            _targetedFields = targetedFields;
            _resourceGraph = resourceGraph;
            _genericProcessorFactory = genericProcessorFactory;
            _context = contextResolver.GetContext();
            _dbSet = _context.Set<TResource>();
        }

        /// <inheritdoc />
        public virtual IQueryable<TResource> Get() => _dbSet;
        /// <inheritdoc />
        public virtual IQueryable<TResource> Get(TId id) => _dbSet.Where(e => e.Id.Equals(id));

        /// <inheritdoc />
        public virtual IQueryable<TResource> Select(IQueryable<TResource> entities, List<AttrAttribute> fields)
        {
            if (fields?.Count > 0)
                return entities.Select(fields);

            return entities;
        }

        /// <inheritdoc />
        public virtual IQueryable<TResource> Filter(IQueryable<TResource> entities, FilterQueryContext filterQueryContext)
        {
            if (filterQueryContext.IsCustom)
            {
                var query = (Func<IQueryable<TResource>, FilterQuery, IQueryable<TResource>>)filterQueryContext.CustomQuery;
                return query(entities, filterQueryContext.Query);
            }
            return entities.Filter(filterQueryContext);
        }

        /// <inheritdoc />
        public virtual IQueryable<TResource> Sort(IQueryable<TResource> entities, SortQueryContext sortQueryContext)
        {
            return entities.Sort(sortQueryContext);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> CreateAsync(TResource entity)
        {
            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                object trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, entity, out bool relationshipWasAlreadyTracked);
                LoadInverseRelationships(trackedRelationshipValue, relationshipAttr);
                if (relationshipWasAlreadyTracked || relationshipAttr is HasManyThroughAttribute)
                    /// We only need to reassign the relationship value to the to-be-added
                    /// entity when we're using a different instance of the relationship (because this different one
                    /// was already tracked) than the one assigned to the to-be-created entity.
                    /// Alternatively, even if we don't have to reassign anything because of already tracked 
                    /// entities, we still need to assign the "through" entities in the case of many-to-many.
                    relationshipAttr.SetValue(entity, trackedRelationshipValue);
            }
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            // this ensures relationships get reloaded from the database if they have
            // been requested. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
            DetachRelationships(entity);

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
                    relationEntry.Reference(hasOneAttr.InverseNavigation).Load();
                else
                    relationEntry.Collection(hasOneAttr.InverseNavigation).Load();
            }
            else if (relationshipAttr is HasManyAttribute hasManyAttr && !(relationshipAttr is HasManyThroughAttribute))
            {
                foreach (IIdentifiable relationshipValue in (IList)trackedRelationshipValue)
                    _context.Entry(relationshipValue).Reference(hasManyAttr.InverseNavigation).Load();
            }
        }

        private bool IsHasOneRelationship(string internalRelationshipName, Type type)
        {
            var relationshipAttr = _resourceGraph.GetRelationships(type).FirstOrDefault(r => r.InternalRelationshipName == internalRelationshipName);
            if (relationshipAttr != null)
            {
                if (relationshipAttr is HasOneAttribute)
                    return true;

                return false;
            }
            // relationshipAttr is null when we don't put a [RelationshipAttribute] on the inverse navigation property.
            // In this case we use relfection to figure out what kind of relationship is pointing back.
            return !type.GetProperty(internalRelationshipName).PropertyType.Inherits(typeof(IEnumerable));
        }

        private void DetachRelationships(TResource entity)
        {
            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                if (relationshipAttr is HasOneAttribute hasOneAttr)
                {
                    var relationshipValue = (IIdentifiable)hasOneAttr.GetValue(entity);
                    if (relationshipValue == null) continue;
                    _context.Entry(relationshipValue).State = EntityState.Detached;
                }
                else
                {
                    IEnumerable<IIdentifiable> relationshipValueList = (IEnumerable<IIdentifiable>)relationshipAttr.GetValue(entity);
                    if (relationshipValueList == null) continue;
                    foreach (var pointer in relationshipValueList)
                        _context.Entry(pointer).State = EntityState.Detached;
                    /// detaching has many relationships is not sufficient to 
                    /// trigger a full reload of relationships: the navigation 
                    /// property actually needs to be nulled out, otherwise
                    /// EF will still add duplicate instances to the collection
                    relationshipAttr.SetValue(entity, null);
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<TResource> UpdateAsync(TResource updatedEntity)
        {
            var databaseEntity = await Get(updatedEntity.Id).FirstOrDefaultAsync();
            if (databaseEntity == null)
                return null;

            foreach (var attribute in _targetedFields.Attributes)
                attribute.SetValue(databaseEntity, attribute.GetValue(updatedEntity));

            foreach (var relationshipAttr in _targetedFields.Relationships)
            {
                /// loads databasePerson.todoItems
                LoadCurrentRelationships(databaseEntity, relationshipAttr);
                /// trackedRelationshipValue is either equal to updatedPerson.todoItems,
                /// or replaced with the same set (same ids) of todoItems from the EF Core change tracker,
                /// which is the case if they were already tracked
                object trackedRelationshipValue = GetTrackedRelationshipValue(relationshipAttr, updatedEntity, out _);
                /// loads into the db context any persons currently related
                /// to the todoItems in trackedRelationshipValue
                LoadInverseRelationships(trackedRelationshipValue, relationshipAttr);
                /// assigns the updated relationship to the database entity
                //AssignRelationshipValue(databaseEntity, trackedRelationshipValue, relationshipAttr);
                relationshipAttr.SetValue(databaseEntity, trackedRelationshipValue);
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
        private object GetTrackedRelationshipValue(RelationshipAttribute relationshipAttr, TResource entity, out bool wasAlreadyAttached)
        {
            wasAlreadyAttached = false;
            if (relationshipAttr is HasOneAttribute hasOneAttr)
            {
                var relationshipValue = (IIdentifiable)hasOneAttr.GetValue(entity);
                if (relationshipValue == null)
                    return null;
                return GetTrackedHasOneRelationshipValue(relationshipValue, hasOneAttr, ref wasAlreadyAttached);
            }

            IEnumerable<IIdentifiable> relationshipValueList = (IEnumerable<IIdentifiable>)relationshipAttr.GetValue(entity);
            if (relationshipValueList == null)
                return null;

            return GetTrackedManyRelationshipValue(relationshipValueList, relationshipAttr, ref wasAlreadyAttached);
        }

        // helper method used in GetTrackedRelationshipValue. See comments below.
        private IList GetTrackedManyRelationshipValue(IEnumerable<IIdentifiable> relationshipValueList, RelationshipAttribute relationshipAttr, ref bool wasAlreadyAttached)
        {
            if (relationshipValueList == null) return null;
            bool _wasAlreadyAttached = false;
            var trackedPointerCollection = relationshipValueList.Select(pointer =>
            {   // convert each element in the value list to relationshipAttr.DependentType.
                var tracked = AttachOrGetTracked(pointer);
                if (tracked != null) _wasAlreadyAttached = true;
                return Convert.ChangeType(tracked ?? pointer, relationshipAttr.RightType);
            }).ToList().Cast(relationshipAttr.RightType);
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
                : relationship.RightType;

            var genericProcessor = _genericProcessorFactory.GetProcessor<IGenericProcessor>(typeof(GenericProcessor<>), typeToUpdate);
            await genericProcessor.UpdateRelationshipsAsync(parent, relationship, relationshipIds);
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = await Get(id).FirstOrDefaultAsync();
            if (entity == null) return false;
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual IQueryable<TResource> Include(IQueryable<TResource> entities, params RelationshipAttribute[] inclusionChain)
        {
            if (!inclusionChain.Any())
                return entities;

            string internalRelationshipPath = null;
            foreach (var relationship in inclusionChain)
                internalRelationshipPath = (internalRelationshipPath == null)
                    ? relationship.RelationshipPath
                    : $"{internalRelationshipPath}.{relationship.RelationshipPath}";

            return entities.Include(internalRelationshipPath);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TResource>> PageAsync(IQueryable<TResource> entities, int pageSize, int pageNumber)
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
        public async Task<int> CountAsync(IQueryable<TResource> entities)
        {
            return (entities is IAsyncEnumerable<TResource>)
                 ? await entities.CountAsync()
                 : entities.Count();
        }

        /// <inheritdoc />
        public async Task<TResource> FirstOrDefaultAsync(IQueryable<TResource> entities)
        {
            return (entities is IAsyncEnumerable<TResource>)
               ? await entities.FirstOrDefaultAsync()
               : entities.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResource>> ToListAsync(IQueryable<TResource> entities)
        {
            return (entities is IAsyncEnumerable<TResource>)
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
        protected void LoadCurrentRelationships(TResource oldEntity, RelationshipAttribute relationshipAttribute)
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
        /// The relationshipValue parameter contains the dependent side of the relationship (Tags).
        /// We can't directly add them to the left entity (Article): we need to 
        /// use the join table (ArticleTags). This methods assigns the relationship value to entity
        /// by taking care of that
        /// </summary>
        private void AssignHasManyThrough(TResource entity, HasManyThroughAttribute hasManyThrough, IList relationshipValue)
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
    public class DefaultResourceRepository<TResource> : DefaultResourceRepository<TResource, int>, IResourceRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public DefaultResourceRepository(ITargetedFields targetedFields,
                                       IDbContextResolver contextResolver,
                                       IResourceGraph contextEntityProvider,
                                       IGenericProcessorFactory genericProcessorFactory)
            : base(targetedFields, contextResolver, contextEntityProvider, genericProcessorFactory)
        {
        }

        public DefaultResourceRepository(ITargetedFields targetedFields,
                                       IDbContextResolver contextResolver,
                                       IResourceGraph contextEntityProvider,
                                       IGenericProcessorFactory genericProcessorFactory,
                                       ILoggerFactory loggerFactory = null)
            : base(targetedFields, contextResolver, contextEntityProvider, genericProcessorFactory, loggerFactory)
        {
        }
    }
}
