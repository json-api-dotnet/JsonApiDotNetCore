using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.Services
{
    public class EntityResourceService<TResource> : EntityResourceService<TResource, int>,
        IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public EntityResourceService(
            IJsonApiContext jsonApiContext,
            IEntityRepository<TResource> entityRepository,
            ILoggerFactory loggerFactory) :
            base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }

    public class EntityResourceService<TResource, TId> : EntityResourceService<TResource, TResource, TId>,
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        public EntityResourceService(
            IJsonApiContext jsonApiContext,
            IEntityRepository<TResource, TId> entityRepository,
            ILoggerFactory loggerFactory) :
            base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }

    public class EntityResourceService<TResource, TEntity, TId> :
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IEntityRepository<TEntity, TId> _entities;
        private readonly ILogger _logger;
        private readonly IResourceMapper _mapper;

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                ILoggerFactory loggerFactory)
        {
            // no mapper provided, TResource & TEntity must be the same type
            if (typeof(TResource) != typeof(TEntity))
            {
                throw new InvalidOperationException("Resource and Entity types are NOT the same. Please provide a mapper.");
            }

            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
        }

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                ILoggerFactory loggerFactory,
                IResourceMapper mapper)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
            _mapper = mapper;
        }

        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            var entity = MapIn(resource);

            entity = await _entities.CreateAsync(entity);

            return MapOut(entity);
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            return await _entities.DeleteAsync(id);
        }

        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
            var entities = _entities.Get();

            entities = ApplySortAndFilterQuery(entities);

            if (ShouldIncludeRelationships())
                entities = IncludeRelationships(entities, _jsonApiContext.QuerySet.IncludedRelationships);

            if (_jsonApiContext.Options.IncludeTotalRecordCount)
                _jsonApiContext.PageManager.TotalRecords = await _entities.CountAsync(entities);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            if (ShouldIncludeRelationships())
                return await GetWithRelationshipsAsync(id);

            TEntity entity = await _entities.GetAsync(id);

            return MapOut(entity);
        }

        public virtual async Task<object> GetRelationshipsAsync(TId id, string relationshipName)
            => await GetRelationshipAsync(id, relationshipName);

        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);

            // TODO: it would be better if we could distinguish whether or not the relationship was not found,
            // vs the relationship not being set on the instance of T
            if (entity == null)
            {
                throw new JsonApiException(404, $"Relationship '{relationshipName}' not found.");
            }

            var resource = MapOut(entity);

            // compound-property -> CompoundProperty
            var navigationPropertyName = _jsonApiContext.ContextGraph.GetRelationshipName<TResource>(relationshipName);
            if (navigationPropertyName == null)
                throw new JsonApiException(422, $"Relationship '{relationshipName}' does not exist on resource '{typeof(TResource)}'.");

            var relationshipValue = _jsonApiContext.ContextGraph.GetRelationship(resource, navigationPropertyName);
            return relationshipValue;
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource)
        {
            var entity = MapIn(resource);

            entity = await _entities.UpdateAsync(id, entity);

            return MapOut(entity);
        }

        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<DocumentData> relationships)
        {
            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);
            if (entity == null)
            {
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");
            }

            var relationship = _jsonApiContext.ContextGraph
                .GetContextEntity(typeof(TResource))
                .Relationships
                .FirstOrDefault(r => r.Is(relationshipName));

            var relationshipType = relationship.Type;

            // update relationship type with internalname
            var entityProperty = typeof(TEntity).GetProperty(relationship.InternalRelationshipName);
            if (entityProperty == null)
            {
                throw new JsonApiException(404, $"Property {relationship.InternalRelationshipName} " +
                    $"could not be found on entity.");
            }

            relationship.Type = relationship.IsHasMany
                ? entityProperty.PropertyType.GetGenericArguments()[0]
                : entityProperty.PropertyType;

            var relationshipIds = relationships.Select(r => r?.Id?.ToString());

            await _entities.UpdateRelationshipsAsync(entity, relationship, relationshipIds);

            relationship.Type = relationshipType;
        }

        protected virtual async Task<IEnumerable<TResource>> ApplyPageQueryAsync(IQueryable<TEntity> entities)
        {
            var pageManager = _jsonApiContext.PageManager;
            if (!pageManager.IsPaginated)
            {
                var allEntities = await _entities.ToListAsync(entities);
                return (typeof(TResource) == typeof(TEntity)) ? allEntities as IEnumerable<TResource> :
                    _mapper.Map<IEnumerable<TResource>>(allEntities);
            }

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation($"Applying paging query. Fetching page {pageManager.CurrentPage} " +
                    $"with {pageManager.PageSize} entities");
            }

            var pagedEntities = await _entities.PageAsync(entities, pageManager.PageSize, pageManager.CurrentPage);

            return MapOut(pagedEntities);
        }

        protected virtual IQueryable<TEntity> ApplySortAndFilterQuery(IQueryable<TEntity> entities)
        {
            var query = _jsonApiContext.QuerySet;

            if (_jsonApiContext.QuerySet == null)
                return entities;

            if (query.Filters.Count > 0)
                foreach (var filter in query.Filters)
                    entities = _entities.Filter(entities, filter);

            if (query.SortParameters != null && query.SortParameters.Count > 0)
                entities = _entities.Sort(entities, query.SortParameters);

            return entities;
        }

        protected virtual IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, List<string> relationships)
        {
            _jsonApiContext.IncludedRelationships = relationships;

            foreach (var r in relationships)
                entities = _entities.Include(entities, r);

            return entities;
        }

        private async Task<TResource> GetWithRelationshipsAsync(TId id)
        {
            var query = _entities.Get().Where(e => e.Id.Equals(id));

            _jsonApiContext.QuerySet.IncludedRelationships.ForEach(r =>
            {
                query = _entities.Include(query, r);
            });

            var value = await _entities.FirstOrDefaultAsync(query);

            return MapOut(value);
        }

        private bool ShouldIncludeRelationships()
            => (_jsonApiContext.QuerySet?.IncludedRelationships != null &&
            _jsonApiContext.QuerySet.IncludedRelationships.Count > 0);

        private TResource MapOut(TEntity entity)
            => (typeof(TResource) == typeof(TEntity))
                ? entity as TResource :
                _mapper.Map<TResource>(entity);

        private IEnumerable<TResource> MapOut(IEnumerable<TEntity> entities)
            => (typeof(TResource) == typeof(TEntity))
                ? entities as IEnumerable<TResource>
                : _mapper.Map<IEnumerable<TResource>>(entities);

        private TEntity MapIn(TResource resource)
            => (typeof(TResource) == typeof(TEntity))
                ? resource as TEntity
                : _mapper.Map<TEntity>(resource);
    }
}
