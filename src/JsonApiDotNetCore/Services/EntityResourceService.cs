using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Entity mapping class
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class EntityResourceService<TResource, TEntity, TId> :
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly IPageQueryService _pageManager;
        private readonly ICurrentRequest _currentRequest;
        private readonly IJsonApiOptions _options;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;
        private readonly IEntityRepository<TEntity, TId> _repository;
        private readonly ILogger _logger;
        private readonly IResourceMapper _mapper;
        private readonly IResourceHookExecutor _hookExecutor;
        private readonly IIncludeService _includeService;
        private readonly ContextEntity _currentRequestResource;

        public EntityResourceService(
                IEntityRepository<TEntity, TId> repository,
                IJsonApiOptions options,
                ITargetedFields updatedFields,
                ICurrentRequest currentRequest,
                IIncludeService includeService,
                IPageQueryService pageManager,
                IResourceGraph resourceGraph,
                IResourceHookExecutor hookExecutor = null,
                IResourceMapper mapper = null,
                ILoggerFactory loggerFactory = null)
        {
            _currentRequest = currentRequest;
            _includeService = includeService;
            _pageManager = pageManager;
            _options = options;
            _targetedFields = updatedFields;
            _resourceGraph = resourceGraph;
            _repository = repository;
            if (mapper == null && typeof(TResource) != typeof(TEntity))
            {
                throw new InvalidOperationException("Resource and Entity types are NOT the same. Please provide a mapper.");
            }
            _hookExecutor = hookExecutor;
            _mapper = mapper;
            _logger = loggerFactory?.CreateLogger<EntityResourceService<TResource, TEntity, TId>>();
            _currentRequestResource = resourceGraph.GetContextEntity<TResource>();
        }

        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            var entity = MapIn(resource);
            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeCreate(AsList(entity), ResourcePipeline.Post).SingleOrDefault();
            entity = await _repository.CreateAsync(entity);

            // this ensures relationships get reloaded from the database if they have
            // been requested
            // https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
            if (ShouldIncludeRelationships())
            {
                if (_repository is IEntityFrameworkRepository<TEntity> efRepository)
                    efRepository.DetachRelationshipPointers(entity);

                entity = await GetWithRelationshipsAsync(entity.Id);

            }
            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterCreate(AsList(entity), ResourcePipeline.Post);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Get).SingleOrDefault();
            }
            return MapOut(entity);
        }
        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
            entity.Id = id;
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.BeforeDelete(AsList(entity), ResourcePipeline.Delete);
            var succeeded = await _repository.DeleteAsync(entity.Id);
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterDelete(AsList(entity), ResourcePipeline.Delete, succeeded);
            return succeeded;
        }
        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
            _hookExecutor?.BeforeRead<TEntity>(ResourcePipeline.Get);
            var entities = _repository.Get();

            entities = ApplySortAndFilterQuery(entities);

            if (ShouldIncludeRelationships())
                entities = IncludeRelationships(entities);

            if (_options.IncludeTotalRecordCount)
                _pageManager.TotalRecords = await _repository.CountAsync(entities);

            entities = _repository.Select(entities, _currentRequest.QuerySet?.Fields);

            if (!IsNull(_hookExecutor, entities))
            {
                var result = entities.ToList();
                _hookExecutor.AfterRead(result, ResourcePipeline.Get);
                entities = _hookExecutor.OnReturn(result, ResourcePipeline.Get).AsQueryable();
            }

            if (_options.IncludeTotalRecordCount)
                _pageManager.TotalRecords = await _repository.CountAsync(entities);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            var pipeline = ResourcePipeline.GetSingle;
            _hookExecutor?.BeforeRead<TEntity>(pipeline, id.ToString());

            TEntity entity;
            if (ShouldIncludeRelationships())
                entity = await GetWithRelationshipsAsync(id);
            else
                entity = await _repository.GetAsync(id);

            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterRead(AsList(entity), pipeline);
                entity = _hookExecutor.OnReturn(AsList(entity), pipeline).SingleOrDefault();
            }
            return MapOut(entity);
        }

        // triggered by GET /articles/1/relationships/{relationshipName}
        public virtual async Task<TResource> GetRelationshipsAsync(TId id, string relationshipName)
        {
            var relationship = GetRelationship(relationshipName);

            // BeforeRead hook execution
            _hookExecutor?.BeforeRead<TEntity>(ResourcePipeline.GetRelationship, id.ToString());

            // TODO: it would be better if we could distinguish whether or not the relationship was not found,
            // vs the relationship not being set on the instance of T
            var entity = await _repository.GetAndIncludeAsync(id, relationship);
            if (entity == null) // this does not make sense. If the parent entity is not found, this error is thrown?
                throw new JsonApiException(404, $"Relationship '{relationshipName}' not found.");

            if (!IsNull(_hookExecutor, entity))
            {   // AfterRead and OnReturn resource hook execution.
                _hookExecutor.AfterRead(AsList(entity), ResourcePipeline.GetRelationship);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.GetRelationship).SingleOrDefault();
            }

            var resource = MapOut(entity);

            return resource;
        }

        // triggered by GET /articles/1/{relationshipName}
        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            var relationship = GetRelationship(relationshipName);
            var resource = await GetRelationshipsAsync(id, relationshipName);
            return _resourceGraph.GetRelationship(resource, relationship.InternalRelationshipName);
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource)
        {
            var entity = MapIn(resource);

            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.Patch).SingleOrDefault();
            entity = await _repository.UpdateAsync(entity);
            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.Patch);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Patch).SingleOrDefault();
            }
            return MapOut(entity);
        }

        // triggered by PATCH /articles/1/relationships/{relationshipName}
        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<ResourceObject> relationships)
        {
            var relationship = GetRelationship(relationshipName);

            var entity = await _repository.GetAndIncludeAsync(id, relationship);
            if (entity == null)
            {
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");
            }

            var relationshipType = relationship.DependentType;

            // update relationship type with internalname
            var entityProperty = typeof(TEntity).GetProperty(relationship.InternalRelationshipName);
            if (entityProperty == null)
            {
                throw new JsonApiException(404, $"Property {relationship.InternalRelationshipName} " +
                    $"could not be found on entity.");
            }

            /// Why are we changing this value on the attribute and setting it back below?
            /// This feels extremely hacky
            relationship.Type = relationship.IsHasMany
                ? entityProperty.PropertyType.GetGenericArguments()[0]
                : entityProperty.PropertyType;

            var relationshipIds = relationships.Select(r => r?.Id?.ToString());

            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.PatchRelationship).SingleOrDefault();
            await _repository.UpdateRelationshipsAsync(entity, relationship, relationshipIds);
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.PatchRelationship);

            relationship.Type = relationshipType;
        }

        protected virtual async Task<IEnumerable<TResource>> ApplyPageQueryAsync(IQueryable<TEntity> entities)
        {
            if (!(_pageManager.PageSize > 0))
            {
                var allEntities = await _repository.ToListAsync(entities);
                return (typeof(TResource) == typeof(TEntity)) ? allEntities as IEnumerable<TResource> :
                    _mapper.Map<IEnumerable<TResource>>(allEntities);
            }

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation($"Applying paging query. Fetching page {_pageManager.CurrentPage} " +
                    $"with {_pageManager.PageSize} entities");
            }

            var pagedEntities = await _repository.PageAsync(entities, _pageManager.PageSize, _pageManager.CurrentPage);

            return MapOut(pagedEntities);
        }

        protected virtual IQueryable<TEntity> ApplySortAndFilterQuery(IQueryable<TEntity> entities)
        {
            var query = _currentRequest.QuerySet;

            if (_currentRequest.QuerySet == null)
                return entities;

            if (query.Filters.Count > 0)
                foreach (var filter in query.Filters)
                    entities = _repository.Filter(entities, filter);

            entities = _repository.Sort(entities, query.SortParameters);

            return entities;
        }

        /// <summary>
        /// Actually includes the relationships
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities)
        {
            foreach (var r in _includeService.Get())
                entities = _repository.Include(entities, r.ToArray());

            return entities;
        }

        /// <summary>
        /// Get the specified id with relationships provided in the post request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<TEntity> GetWithRelationshipsAsync(TId id)
        {
            var query = _repository.Select(_repository.Get(), _currentRequest.QuerySet?.Fields).Where(e => e.Id.Equals(id));

            foreach (var chain in _includeService.Get())
                query = _repository.Include(query, chain.ToArray());

            TEntity value;
            // https://github.com/aspnet/EntityFrameworkCore/issues/6573
            if (_targetedFields.Attributes.Count() > 0)
                value = query.FirstOrDefault();
            else
                value = await _repository.FirstOrDefaultAsync(query);


            return value;
        }

        private bool ShouldIncludeRelationships()
        {
            return _includeService.Get().Count() > 0;
        }


        private bool IsNull(params object[] values)
        {
            foreach (var val in values)
            {
                if (val == null) return true;
            }
            return false;
        }

        private RelationshipAttribute GetRelationship(string relationshipName)
        {
            var relationship = _currentRequestResource.Relationships.Single(r => r.Is(relationshipName));
            if (relationship == null)
                throw new JsonApiException(422, $"Relationship '{relationshipName}' does not exist on resource '{typeof(TResource)}'.");
            return relationship;
        }

        /// <summary>
        /// Casts the entity given to `TResource` or maps it to its equal
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private TResource MapOut(TEntity entity)
        {
            return (typeof(TResource) == typeof(TEntity)) ? entity as TResource : _mapper.Map<TResource>(entity);
        }

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
    /// <summary>
    /// No mapping
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class EntityResourceService<TResource, TId> : EntityResourceService<TResource, TResource, TId>,
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        public EntityResourceService(IEntityRepository<TResource, TId> repository, IJsonApiOptions options, ITargetedFields updatedFields, ICurrentRequest currentRequest, IIncludeService includeService, IPageQueryService pageManager, IResourceGraph resourceGraph, IResourceHookExecutor hookExecutor = null, IResourceMapper mapper = null, ILoggerFactory loggerFactory = null) : base(repository, options, updatedFields, currentRequest, includeService, pageManager, resourceGraph, hookExecutor, mapper, loggerFactory)
        {
        }
    }

    /// <summary>
    /// No mapping with integer as default
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class EntityResourceService<TResource> : EntityResourceService<TResource, int>,
        IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public EntityResourceService(IEntityRepository<TResource, int> repository, IJsonApiOptions options, ITargetedFields updatedFields, ICurrentRequest currentRequest, IIncludeService includeService, IPageQueryService pageManager, IResourceGraph resourceGraph, IResourceHookExecutor hookExecutor = null, IResourceMapper mapper = null, ILoggerFactory loggerFactory = null) : base(repository, options, updatedFields, currentRequest, includeService, pageManager, resourceGraph, hookExecutor, mapper, loggerFactory)
        {
        }
    }
}
