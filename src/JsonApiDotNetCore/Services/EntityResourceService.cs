using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
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
            ILoggerFactory loggerFactory = null,
            IResourceHookExecutor hookExecutor = null) :
            base(jsonApiContext, entityRepository, loggerFactory, hookExecutor)
        { }
    }

    public class EntityResourceService<TResource, TId> : EntityResourceService<TResource, TResource, TId>,
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        public EntityResourceService(
            IJsonApiContext jsonApiContext,
            IEntityRepository<TResource, TId> entityRepository,
            ILoggerFactory loggerFactory = null, 
            IResourceHookExecutor hookExecutor = null) :
            base(jsonApiContext, entityRepository, hookExecutor, loggerFactory)
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
        private readonly IResourceHookExecutor _hookExecutor;

        public EntityResourceService(
        IJsonApiContext jsonApiContext,
        IEntityRepository<TEntity, TId> entityRepository,
        IResourceHookExecutor hookExecutor,
        ILoggerFactory loggerFactory = null)
        {
            // no mapper provided, TResource & TEntity must be the same type
            if (typeof(TResource) != typeof(TEntity))
            {
                throw new InvalidOperationException("Resource and Entity types are NOT the same. Please provide a mapper.");
            }

            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _hookExecutor = hookExecutor;
            _logger = loggerFactory?.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
        }

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                ILoggerFactory loggerFactory = null)
        {
            // no mapper provided, TResource & TEntity must be the same type
            if (typeof(TResource) != typeof(TEntity))
            {
                throw new InvalidOperationException("Resource and Entity types are NOT the same. Please provide a mapper.");
            }

            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory?.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
        }

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                IResourceMapper mapper,
                ILoggerFactory loggerFactory = null)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
            _mapper = mapper;
        }

        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            var entity = MapIn(resource);

            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeCreate(AsList(entity), ResourcePipeline.Create).SingleOrDefault();
            entity = await _entities.CreateAsync(entity);

            // this ensures relationships get reloaded from the database if they have
            // been requested
            // https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
            if (ShouldIncludeRelationships())
            {
                if (_entities is IEntityFrameworkRepository<TEntity> efRepository)
                    efRepository.DetachRelationshipPointers(entity);

                entity = await GetWithRelationshipsAsync(entity.Id);

            }
            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterCreate(AsList(entity), ResourcePipeline.Create);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Read).SingleOrDefault();
            }
            return MapOut(entity);
        }
        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = await _entities.GetAsync(id);
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.BeforeDelete(AsList(entity), ResourcePipeline.Delete);
            var succeeded = await _entities.DeleteAsync(entity);
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterDelete(AsList(entity), ResourcePipeline.Delete, succeeded);
            return succeeded;
        }

        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
            _hookExecutor?.BeforeRead<TEntity>(ResourcePipeline.Read);
            var entities = _entities.Get();

            entities = ApplySortAndFilterQuery(entities);

            if (ShouldIncludeRelationships())
                entities = IncludeRelationships(entities, _jsonApiContext.QuerySet.IncludedRelationships);
                
            if (_jsonApiContext.Options.IncludeTotalRecordCount)
                _jsonApiContext.PageManager.TotalRecords = await _entities.CountAsync(entities);

            entities = _entities.Select(entities, _jsonApiContext.QuerySet?.Fields);

            if (!IsNull(_hookExecutor, entities))
            {
                var result = entities.ToList();
                _hookExecutor.AfterRead(result, ResourcePipeline.Read);
                entities = _hookExecutor.OnReturn(result, ResourcePipeline.Read).AsQueryable();
            }

            if (_jsonApiContext.Options.IncludeTotalRecordCount)
                _jsonApiContext.PageManager.TotalRecords = await _entities.CountAsync(entities);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            var pipeline = ResourcePipeline.ReadSingle;
            _hookExecutor?.BeforeRead<TEntity>(pipeline, id.ToString());
            TEntity entity;
            if (ShouldIncludeRelationships())
            {
                entity = await GetWithRelationshipsAsync(id);
            }
            else
            {
                entity = await _entities.GetAsync(id);
            }
            if(!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterRead(AsList(entity), pipeline);
                entity = _hookExecutor.OnReturn(AsList(entity), pipeline).SingleOrDefault();
            }
            return MapOut(entity);

        }

        // triggered by GET /articles/1/relationships/{relationshipName}
        public virtual async Task<object> GetRelationshipsAsync(TId id, string relationshipName) => await GetRelationshipAsync(id, relationshipName);

        // triggered by GET /articles/1/{relationshipName}
        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {

            _hookExecutor?.BeforeRead<TEntity>(ResourcePipeline.ReadRelationship, id.ToString());
            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);
            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterRead(AsList(entity), ResourcePipeline.ReadRelationship);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.ReadRelationship).SingleOrDefault();
            }

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


            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.Patch).SingleOrDefault();
            entity = await _entities.UpdateAsync(id, entity);
            if (!IsNull(_hookExecutor, entity))
            {
                // TODO: should not fire after read for L=0
                _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.Patch);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Patch).SingleOrDefault();
            }

            return MapOut(entity);
        }

        // triggered by PATCH /articles/1/relationships/{relationshipName}
        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<ResourceObject> relationships)
        {
            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);
            if (entity == null)
            {
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");
            }

            var relationship = _jsonApiContext.ResourceGraph
                .GetContextEntity(typeof(TResource))
                .Relationships
                .FirstOrDefault(r => r.Is(relationshipName));

            var relationshipType = relationship.DependentType;

            // update relationship type with internalname
            var entityProperty = typeof(TEntity).GetProperty(relationship.InternalRelationshipName);
            if (entityProperty == null)
            {
                throw new JsonApiException(404, $"Property {relationship.InternalRelationshipName} " +
                    $"could not be found on entity.");
            }

            /// Why are we changing this value on the attribute and setting it back below? This feels very hacky
            relationship.Type = relationship.IsHasMany
                ? entityProperty.PropertyType.GetGenericArguments()[0]
                : entityProperty.PropertyType;

            var relationshipIds = relationships.Select(r => r?.Id?.ToString());

            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.PatchRelationship).SingleOrDefault();
            await _entities.UpdateRelationshipsAsync(entity, relationship, relationshipIds);
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.PatchRelationship);

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

        private async Task<TEntity> GetWithRelationshipsAsync(TId id)
        {
            var query = _entities.Select(_entities.Get(), _jsonApiContext.QuerySet?.Fields).Where(e => e.Id.Equals(id));

            _jsonApiContext.QuerySet.IncludedRelationships.ForEach(r =>
            {
                query = _entities.Include(query, r);
            });

            TEntity value;
            // https://github.com/aspnet/EntityFrameworkCore/issues/6573
            if (_jsonApiContext.QuerySet?.Fields?.Count > 0)
                value = query.FirstOrDefault();
            else
                value = await _entities.FirstOrDefaultAsync(query);

            return value;
        }


        private bool IsNull(params object[] values)
        {
            foreach (var val in values)
            {
                if (val == null) return true;
            }
            return false;
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

        private List<TEntity> AsList(TEntity entity)
        {
            return new List<TEntity> { entity };
        }
    }
}
