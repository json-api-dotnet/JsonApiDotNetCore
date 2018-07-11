using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    public class MappingResourceService<TDto, TEntity> :
    MappingResourceService<TDto, TEntity, int>,
    IResourceService<TDto>
        where TDto : class, IIdentifiable<int>
        where TEntity : class, IIdentifiable<int>
    {
        public MappingResourceService(
          IJsonApiContext jsonApiContext,
          IEntityRepository<TEntity> entityRepository,
          ILoggerFactory loggerFactory,
          IMapper mapper)
          : base(jsonApiContext, entityRepository, loggerFactory, mapper)
        { }
    }

    public class MappingResourceService<TDto, TEntity, TId> : IResourceService<TDto, TId>
        where TDto : class, IIdentifiable<TId>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IEntityRepository<TEntity, TId> _entities;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public MappingResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                ILoggerFactory loggerFactory,
                IMapper mapper)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory.CreateLogger<MappingResourceService<TDto, TEntity, TId>>();
            _mapper = mapper;
        }

        public virtual async Task<IEnumerable<TDto>> GetAsync()
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

        public virtual async Task<TDto> GetAsync(TId id)
        {
            TDto entity;
            if (ShouldIncludeRelationships())
                entity = await GetWithRelationshipsAsync(id);
            else
                entity = _mapper.Map<TDto>(await _entities.GetAsync(id));
            return entity;
        }

        private bool ShouldIncludeRelationships()
            => (_jsonApiContext.QuerySet?.IncludedRelationships != null && _jsonApiContext.QuerySet.IncludedRelationships.Count > 0);

        private async Task<TDto> GetWithRelationshipsAsync(TId id)
        {
            var query = _entities.Get().Where(e => e.Id.Equals(id));
            _jsonApiContext.QuerySet.IncludedRelationships.ForEach(r =>
            {
                query = _entities.Include(query, r);
            });
            var value = await _entities.FirstOrDefaultAsync(query);
            return _mapper.Map<TDto>(value);
        }

        public virtual async Task<object> GetRelationshipsAsync(TId id, string relationshipName)
            => await GetRelationshipAsync(id, relationshipName);

        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            relationshipName = _jsonApiContext.ContextGraph
                    .GetRelationshipName<TDto>(relationshipName);

            if (relationshipName == null)
                throw new JsonApiException(422, "Relationship name not specified.");
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace($"Looking up '{relationshipName}'...");
            }

            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);
            // TODO: it would be better if we could distinguish whether or not the relationship was not found,
            // vs the relationship not being set on the instance of T
            if (entity == null)
                throw new JsonApiException(404, $"Relationship {relationshipName} not found.");

            var relationship = _jsonApiContext.ContextGraph
                    .GetRelationship(entity, relationshipName);

            return relationship;
        }

        public virtual async Task<TDto> CreateAsync(TDto entity)
        {
            var createdEntity = await _entities.CreateAsync(_mapper.Map<TEntity>(entity));
            return _mapper.Map<TDto>(createdEntity);
        }

        public virtual async Task<TDto> UpdateAsync(TId id, TDto entity)
        {
            var updatedEntity = await _entities.UpdateAsync(id, _mapper.Map<TEntity>(entity));
            return _mapper.Map<TDto>(updatedEntity);
        }

        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<DocumentData> relationships)
        {
            relationshipName = _jsonApiContext.ContextGraph
                      .GetRelationshipName<TDto>(relationshipName);

            if (relationshipName == null)
                throw new JsonApiException(422, "Relationship name not specified.");

            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);

            if (entity == null)
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");

            var relationship = _jsonApiContext.ContextGraph
                .GetContextEntity(typeof(TDto))
                .Relationships
                .FirstOrDefault(r => r.InternalRelationshipName == relationshipName);

            var relationshipIds = relationships.Select(r => r?.Id?.ToString());

            await _entities.UpdateRelationshipsAsync(entity, relationship, relationshipIds);
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            return await _entities.DeleteAsync(id);
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

        protected virtual async Task<IEnumerable<TDto>> ApplyPageQueryAsync(IQueryable<TEntity> entities)
        {
            var pageManager = _jsonApiContext.PageManager;
            if (!pageManager.IsPaginated)
                return _mapper.Map<IEnumerable<TDto>>(await _entities.ToListAsync(entities));

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation($"Applying paging query. Fetching page {pageManager.CurrentPage} with {pageManager.PageSize} entities");
            }

            return _mapper.Map<IEnumerable<TDto>>(await _entities.PageAsync(entities, pageManager.PageSize, pageManager.CurrentPage));
        }

        protected virtual IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, List<string> relationships)
        {
            _jsonApiContext.IncludedRelationships = relationships;

            foreach (var r in relationships)
                entities = _entities.Include(entities, r);

            return entities;
        }
    }
}
