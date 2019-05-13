using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
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
            IJsonApiOptions options,
            IQueryManager queryManager,
            IPageManager pageManager,
            ILoggerFactory loggerFactory = null) :
            base(jsonApiContext, entityRepository, options, queryManager, pageManager, loggerFactory)
        { }
    }

    public class EntityResourceService<TResource, TId> : EntityResourceService<TResource, TResource, TId>,
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        public EntityResourceService(
            IJsonApiContext jsonApiContext,
            IEntityRepository<TResource, TId> entityRepository,
            IJsonApiOptions apiOptions,
            IQueryManager queryManager,
            IPageManager pageManager,
            ILoggerFactory loggerFactory = null)
            : base(jsonApiContext, entityRepository, apiOptions, queryManager, pageManager, loggerFactory)
        { }
    }

    public class EntityResourceService<TResource, TEntity, TId> :
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly IPageManager _pageManager;
        private readonly IQueryManager _queryManager;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IJsonApiOptions _options;
        private readonly IEntityRepository<TEntity, TId> _repository;
        private readonly ILogger _logger;
        private readonly IResourceMapper _mapper;

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                IJsonApiOptions apiOptions,
                IQueryManager queryManager,
                IPageManager pageManager,
                ILoggerFactory loggerFactory = null) : this(jsonApiContext, entityRepository, apiOptions, null, queryManager, pageManager, loggerFactory )
        {
            // no mapper provided, TResource & TEntity must be the same type
            if (typeof(TResource) != typeof(TEntity))
            {
                throw new InvalidOperationException("Resource and Entity types are NOT the same. Please provide a mapper.");
            }
        }

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                IJsonApiOptions options,
                IResourceMapper mapper,
                IQueryManager queryManager,
                IPageManager pageManager,
                ILoggerFactory loggerFactory)
        {
            _pageManager = pageManager;
            _queryManager = queryManager;
            _jsonApiContext = jsonApiContext;
            _options = options;
            _repository = entityRepository;
            if(loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
            }
            _mapper = mapper;
        }

        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            var entity = MapIn(resource);

            try
            {
            entity = await _repository.CreateAsync(entity);

            }
            catch(DbUpdateException ex)
            {
                throw new JsonApiException(500, "Database update exception", ex);
            }

            // this ensures relationships get reloaded from the database if they have
            // been requested
            // https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
            if (AreRelationshipsIncluded())
            {
                if (_repository is IEntityFrameworkRepository<TEntity> efRepository)
                    efRepository.DetachRelationshipPointers(entity);

                return await GetWithRelationshipsAsync(entity.Id);
            }

            return MapOut(entity);
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            return await _repository.DeleteAsync(id);
        }
        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
            var entities = _repository.GetQueryable();

            entities = ApplySortAndFilterQuery(entities);

            if (AreRelationshipsIncluded())
            {
                entities = IncludeRelationships(entities, _jsonApiContext.QuerySet.IncludedRelationships);
            }

            if (_options.IncludeTotalRecordCount)
            {
                _jsonApiContext.PageManager.TotalRecords = await _repository.CountAsync(entities);
            }

            entities = _repository.Select(entities, _jsonApiContext.QuerySet?.Fields);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            
            TResource resource;
            if (AreRelationshipsIncluded())
            {
                resource = await GetWithRelationshipsAsync(id);
            }
            else
            {
                resource = MapOut(await _repository.GetAsync(id));
            }
            if(resource == null)
            {
                throw new JsonApiException(404, $"That entity ({_jsonApiContext.RequestEntity.EntityName}) with id ({id}) was not found in the database");
            }
            return resource;
        }

        public virtual async Task<object> GetRelationshipsAsync(TId id, string relationshipName)
            => await GetRelationshipAsync(id, relationshipName);

        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            var entity = await _repository.GetAndIncludeAsync(id, relationshipName);

            // TODO: it would be better if we could distinguish whether or not the relationship was not found,
            // vs the relationship not being set on the instance of T
            if (entity == null)
            {
                throw new JsonApiException(404, $"Relationship '{relationshipName}' not found.");
            }

            var resource = MapOut(entity);

            // compound-property -> CompoundProperty
            var navigationPropertyName = _jsonApiContext.ResourceGraph.GetRelationshipName<TResource>(relationshipName);
            if (navigationPropertyName == null)
                throw new JsonApiException(422, $"Relationship '{relationshipName}' does not exist on resource '{typeof(TResource)}'.");

            var relationshipValue = _jsonApiContext.ResourceGraph.GetRelationship(resource, navigationPropertyName);
            return relationshipValue;
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource)
        {
            var entity = MapIn(resource);

            entity = await _repository.UpdateAsync(id, entity);

            return MapOut(entity);
        }

        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<ResourceObject> relationships)
        {
            var entity = await _repository.GetAndIncludeAsync(id, relationshipName);
            if (entity == null)
            {
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");
            }

            var relationship = _jsonApiContext.ResourceGraph
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

            await _repository.UpdateRelationshipsAsync(entity, relationship, relationshipIds);

            relationship.Type = relationshipType;
        }

        protected virtual async Task<IEnumerable<TResource>> ApplyPageQueryAsync(IQueryable<TEntity> entities)
        {
            var pageManager = _jsonApiContext.PageManager;
            if (!pageManager.IsPaginated)
            {
                var allEntities = await _repository.ToListAsync(entities);
                return (typeof(TResource) == typeof(TEntity)) ? allEntities as IEnumerable<TResource> :
                    _mapper.Map<IEnumerable<TResource>>(allEntities);
            }

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation($"Applying paging query. Fetching page {pageManager.CurrentPage} " +
                    $"with {pageManager.PageSize} entities");
            }

            var pagedEntities = await _repository.PageAsync(entities, pageManager.PageSize, pageManager.CurrentPage);

            return MapOut(pagedEntities);
        }

        protected virtual IQueryable<TEntity> ApplySortAndFilterQuery(IQueryable<TEntity> entities)
        {
            var query = _jsonApiContext.QuerySet;

            if (_jsonApiContext.QuerySet == null)
                return entities;

            if (query.Filters.Count > 0)
                foreach (var filter in query.Filters)
                    entities = _repository.Filter(entities, filter);

            entities = _repository.Sort(entities, query.SortParameters);

            return entities;
        }

        /// <summary>
        /// actually include the relationships
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="relationships"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, List<string> relationships)
        {
            _jsonApiContext.IncludedRelationships = relationships;

            foreach (var r in relationships)
            {
                entities = _repository.Include(entities, r);
            }

            return entities;
        }

        /// <summary>
        /// Get the specified id with relationships (async)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<TResource> GetWithRelationshipsAsync(TId id)
        {
            var fields = _queryManager.GetFields();
            var query = _repository.Select(_repository.GetQueryable(), fields).Where(e => e.Id.Equals(id));

            _queryManager.GetRelationships().ForEach(r =>
            {
                query = _repository.Include(query, r);
            });

            TEntity value;
            // https://github.com/aspnet/EntityFrameworkCore/issues/6573
            if(_queryManager.GetFields()?.Count() > 0)
            {
                value = query.FirstOrDefault();
            }
            else
            {
                value = await _repository.FirstOrDefaultAsync(query);
            }
            return MapOut(value);
        }

        /// <summary>
        /// Should the relationships be included?
        /// </summary>
        /// <returns></returns>
        private bool AreRelationshipsIncluded()
        {
            return _queryManager.GetRelationships()?.Count() > 0;

        }
        /// <summary>
        /// Casts the entity given to `TResource` or maps it to its equal
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private TResource MapOut(TEntity entity)
        {
            return (typeof(TResource) == typeof(TEntity))  ? entity as TResource :  _mapper.Map<TResource>(entity);
        }

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
