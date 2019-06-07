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
            AttachRelationships(entity);
            _dbSet.Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        protected virtual void AttachRelationships(TEntity entity = null)
        {
            AttachHasManyPointers(entity);
            AttachHasOnePointers(entity);
        }

        /// <inheritdoc />
        public void DetachRelationshipPointers(TEntity entity)
        {
            foreach (var hasOneRelationship in _jsonApiContext.HasOneRelationshipPointers.Get())
            {
                var hasOne = (HasOneAttribute)hasOneRelationship.Key;
                if (hasOne.EntityPropertyName != null)
                {
                    var relatedEntity = entity.GetType().GetProperty(hasOne.EntityPropertyName)?.GetValue(entity);
                    if (relatedEntity != null)
                        _context.Entry(relatedEntity).State = EntityState.Detached;
                }
                else
                {
                    _context.Entry(hasOneRelationship.Value).State = EntityState.Detached;
                }
            }

            foreach (var hasManyRelationship in _jsonApiContext.HasManyRelationshipPointers.Get())
            {
                var hasMany = (HasManyAttribute)hasManyRelationship.Key;
                if (hasMany.EntityPropertyName != null)
                {
                    var relatedList = (IList)entity.GetType().GetProperty(hasMany.EntityPropertyName)?.GetValue(entity);
                    foreach (var related in relatedList)
                    {
                        _context.Entry(related).State = EntityState.Detached;
                    }
                }
                else
                {
                    foreach (var pointer in hasManyRelationship.Value)
                    {
                        _context.Entry(pointer).State = EntityState.Detached;
                    }
                }

                // HACK: detaching has many relationships doesn't appear to be sufficient
                // the navigation property actually needs to be nulled out, otherwise
                // EF adds duplicate instances to the collection
                hasManyRelationship.Key.SetValue(entity, null);
            }
        }

        /// <summary>
        /// This is used to allow creation of HasMany relationships when the
        /// dependent side of the relationship already exists.
        /// </summary>
        private void AttachHasManyAndHasManyThroughPointers(TEntity entity)
        {
            var relationships = _jsonApiContext.HasManyRelationshipPointers.Get();

            foreach (var attribute in relationships.Keys)
            {
                IEnumerable<IIdentifiable> pointers;
                if (attribute is HasManyThroughAttribute hasManyThrough)
                {
                    pointers = relationships[attribute].Cast<IIdentifiable>();
                }
                else
                {
                    pointers = GetRelationshipPointers(entity, (HasManyAttribute)attribute) ?? relationships[attribute].Cast<IIdentifiable>();
                }

                if (pointers == null) continue;
                bool alreadyTracked = false;
                var newPointerCollection = pointers.Select(pointer =>
                {
                    var tracked = AttachOrGetTracked(pointer);
                    if (tracked != null) alreadyTracked = true;
                    return tracked ?? pointer;
                });

                if (alreadyTracked) relationships[attribute] = (IList)newPointerCollection;
            }
        }

        private void AttachHasMany(TEntity entity, RelationshipAttribute attribute, Dictionary<RelationshipAttribute, IList> relationships)
        {
            IEnumerable<IIdentifiable> pointers;
            if (attribute is HasManyThroughAttribute hasManyThrough)
            {
                pointers = relationships[attribute].Cast<IIdentifiable>();
            } else
            {
                pointers = GetRelationshipPointers(entity, (HasManyAttribute)attribute) ?? relationships[attribute].Cast<IIdentifiable>();
            }

            if (pointers == null) return;
            bool alreadyTracked = false;
            var newPointerCollection = pointers.Select(pointer =>
            {
                var tracked = AttachOrGetTracked(pointer);
                if (tracked != null) alreadyTracked = true;
                return tracked ?? pointer;
            });

            if (alreadyTracked) relationships[attribute] = (IList)newPointerCollection;
        }

        private void AttachHasManyThrough(TEntity entity, HasManyThroughAttribute hasManyThrough, Dictionary<RelationshipAttribute, IList> relationships)
        {
            var pointers = relationships[hasManyThrough].Cast<IIdentifiable>();
            bool alreadyTracked = false;
            var newPointerCollection = pointers.Select(pointer =>
            {
                var tracked = AttachOrGetTracked(pointer);
                if (tracked != null) alreadyTracked = true;
                return tracked ?? pointer;
            });
            if (alreadyTracked) relationships[hasManyAttribute] = (IList)newPointerCollection;

            foreach (var pointer in pointers)
            {
                if (_context.EntityIsTracked(pointer as IIdentifiable) == false)
                    _context.Entry(pointer).State = EntityState.Unchanged;

                var throughRelationshipCollection = Activator.CreateInstance(hasManyThrough.ThroughProperty.PropertyType) as IList;
                hasManyThrough.ThroughProperty.SetValue(entity, throughRelationshipCollection);

                foreach (var pointer in pointers)
                {
                    if (_context.EntityIsTracked(pointer as IIdentifiable) == false)
                        _context.Entry(pointer).State = EntityState.Unchanged;
                    var throughInstance = Activator.CreateInstance(hasManyThrough.ThroughType);

                    hasManyThrough.LeftProperty.SetValue(throughInstance, entity);
                    hasManyThrough.RightProperty.SetValue(throughInstance, pointer);

                    throughRelationshipCollection.Add(throughInstance);
                }
            }
        }

        /// <summary>
        /// This is used to allow creation of HasOne relationships when the
        /// </summary>
        private void AttachHasOnePointers(TEntity entity)
        {
            var relationships = _jsonApiContext
                                    .HasOneRelationshipPointers
                                    .Get();

            foreach (var relationship in relationships)
            {
                var pointer = GetRelationshipPointer(entity, relationship);
                if (pointer == null) return;
                var tracked = AttachOrGetTracked(pointer);
                if (tracked != null) relationships[relationship.Key] = tracked;
            }
        }

        IIdentifiable GetRelationshipPointer(TEntity principalEntity, KeyValuePair<HasOneAttribute, IIdentifiable> relationship)
        {
            HasOneAttribute hasOne = relationship.Key;
            if (hasOne.EntityPropertyName != null)
            {
                var relatedEntity = principalEntity.GetType().GetProperty(hasOne.EntityPropertyName)?.GetValue(principalEntity);
                return (IIdentifiable)relatedEntity;
            }
            return relationship.Value;
        }

        IEnumerable<IIdentifiable> GetRelationshipPointers(TEntity entity, HasManyAttribute attribute)
        {
            if (attribute.EntityPropertyName == null)
            {
                return null;
            }
            return ((IEnumerable)(entity.GetType().GetProperty(attribute.EntityPropertyName)?.GetValue(entity))).Cast<IIdentifiable>();
        }

        // useful article: https://stackoverflow.com/questions/30987806/dbset-attachentity-vs-dbcontext-entryentity-state-entitystate-modified
        IIdentifiable AttachOrGetTracked(IIdentifiable pointer)
        {
            var trackedEntity = _context.GetTrackedEntity(pointer);


            if (trackedEntity != null)
            {
                /// there already was an instance of this type and ID tracked
                /// by EF Core. Reattaching will produce a conflict, and from now on we 
                /// will use the already attached one instead. (This entry might
                /// contain updated fields as a result of business logic)
                return trackedEntity;
            }

            /// the relationship pointer is new to EF Core, but we are sure
            /// it exists in the database (json:api spec), so we attach it.
            _context.Entry(pointer).State = EntityState.Unchanged;
            return null;
        }


        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TId id, TEntity entity)
        {
            var oldEntity = await GetAsync(id);

            if (oldEntity == null)
                return null;

            foreach (var attr in _jsonApiContext.AttributesToUpdate)
                attr.Key.SetValue(oldEntity, attr.Value);

            if (_jsonApiContext.RelationshipsToUpdate.Any())
            {
                /// For one-to-many and many-to-many, the PATCH must perform a 
                /// complete replace. When assigning new relationship values, 
                /// it will only be like this if the to-be-replaced entities are loaded
                foreach (var relationship in _jsonApiContext.RelationshipsToUpdate)
                {
                    if (relationship.Key is HasManyThroughAttribute throughAttribute)
                    {
                        await _context.Entry(oldEntity).Collection(throughAttribute.InternalThroughName).LoadAsync();
                    }
                }

                /// @HACK @TODO: It is inconsistent that for many-to-many, the new relationship value
                /// is assigned in AttachRelationships() helper fn below, but not for 
                /// one-to-many and one-to-one (we need to do that manually as done below).
                /// Simultaneously, for a proper working "complete replacement", in the case of many-to-many
                /// we need to LoadAsync() BEFORE calling AttachRelationships(), but for one-to-many we 
                /// need to do it AFTER AttachRelationships or we we'll get entity tracking errors
                /// This really needs a refactor.
                AttachRelationships(oldEntity);

                foreach (var relationship in _jsonApiContext.RelationshipsToUpdate)
                {
                    if ((relationship.Key.TypeId as Type).IsAssignableFrom(typeof(HasOneAttribute)))
                    {
                        relationship.Key.SetValue(oldEntity, relationship.Value);
                    }
                    if ((relationship.Key.TypeId as Type).IsAssignableFrom(typeof(HasManyAttribute)))
                    {
                        await _context.Entry(oldEntity).Collection(relationship.Key.InternalRelationshipName).LoadAsync();
                        var value = PreventReattachment((IEnumerable<object>)relationship.Value);
                        relationship.Key.SetValue(oldEntity, value);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return oldEntity;
        }

        /// <summary>
        /// We need to make sure we're not re-attaching entities when assigning 
        /// new relationship values. Entities may have been loaded in the change
        /// tracker anywhere in the application beyond the control of
        /// JsonApiDotNetCore.
        /// </summary>
        /// <returns>The interpolated related entity collection</returns>
        /// <param name="relatedEntities">Related entities.</param>
        object PreventReattachment(IEnumerable<object> relatedEntities)
        {
            var relatedType = TypeHelper.GetTypeOfList(relatedEntities.GetType());
            var replaced = relatedEntities.Cast<IIdentifiable>().Select(entity => _context.GetTrackedEntity(entity) ?? entity);
            return TypeHelper.ConvertCollection(replaced, relatedType);

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

            if (entity == null)
                return false;

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
    }
}
