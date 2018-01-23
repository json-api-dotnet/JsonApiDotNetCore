using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    public class EntityResourceService<T>
        : EntityResourceService<T, int>,
        IResourceService<T>
        where T : class, IIdentifiable<int>
    {
        public EntityResourceService(
          IJsonApiContext jsonApiContext,
          IEntityRepository<T> entityRepository,
          ILoggerFactory loggerFactory)
          : base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }

    public class EntityResourceService<T, TId> : IResourceService<T, TId>
        where T : class, IIdentifiable<TId>
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IEntityRepository<T, TId> _entities;
        private readonly ILogger _logger;

        public EntityResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<T, TId> entityRepository,
                ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory.CreateLogger<EntityResourceService<T, TId>>();
        }

        public virtual async Task<IEnumerable<T>> GetAsync()
        {
            var entities = GetReadonly();

            entities = ApplySortAndFilterQuery(entities);

            if (ShouldIncludeRelationships())
                entities = IncludeRelationships(entities, _jsonApiContext.QuerySet.IncludedRelationships);

            if (_jsonApiContext.Options.IncludeTotalRecordCount)
                _jsonApiContext.PageManager.TotalRecords = await _entities.CountAsync(entities);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);
            return pagedEntities;
        }

        public virtual async Task<T> GetAsync(TId id)
        {
            T entity;
            if (ShouldIncludeRelationships())
                entity = await GetWithRelationshipsAsync(id);
            else
                entity = await GetByIdReadonlyAsync(id);

            return entity;
        }

        private bool ShouldIncludeRelationships()
            => (_jsonApiContext.QuerySet?.IncludedRelationships != null && _jsonApiContext.QuerySet.IncludedRelationships.Count > 0);

        private async Task<T> GetWithRelationshipsAsync(TId id)
        {
            var query = GetReadonly().Where(e => e.Id.Equals(id));
            _jsonApiContext.QuerySet.IncludedRelationships.ForEach(r =>
            {
                query = _entities.Include(query, r);
            });

            return await _entities.FirstOrDefaultAsync(query);
        }

        public virtual async Task<object> GetRelationshipsAsync(TId id, string relationshipName)
        {
            _jsonApiContext.IsRelationshipData = true;
            return await GetRelationshipAsync(id, relationshipName);
        }

        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            relationshipName = _jsonApiContext.ContextGraph
                    .GetRelationshipName<T>(relationshipName);

            if (relationshipName == null)
                throw new JsonApiException(422, "Relationship name not specified.");

            _logger.LogTrace($"Looking up '{relationshipName}'...");

            var entity = await GetAndIncludeReadonlyAsync(id, relationshipName);
            if (entity == null)
                throw new JsonApiException(404, $"Relationship {relationshipName} not found.");

            var relationship = _jsonApiContext.ContextGraph
                    .GetRelationship(entity, relationshipName);

            return relationship;
        }

        /// <summary>
        /// This is a temporary measure to maintain backwards API compatibility. 
        /// It is expected that this method will be removed in the next major release.
        /// </summary>
        private IQueryable<T> GetReadonly() => 
            (_entities is DefaultEntityRepository<T, TId>) 
                ? ((DefaultEntityRepository<T, TId>)_entities).Get(isReadonly: true)
                : _entities.Get();

        /// <summary>
        /// This is a temporary measure to maintain backwards API compatibility. 
        /// It is expected that this method will be removed in the next major release.
        /// </summary>
        private async Task<T> GetByIdReadonlyAsync(TId id) => 
            await (
                (_entities is DefaultEntityRepository<T, TId>) 
                    ? ((DefaultEntityRepository<T, TId>)_entities).GetAsync(id, isReadonly: true)
                    : _entities.GetAsync(id)
            );

        /// <summary>
        /// This is a temporary measure to maintain backwards API compatibility. 
        /// It is expected that this method will be removed in the next major release.
        /// </summary>
        private async Task<T> GetAndIncludeReadonlyAsync(TId id, string relationshipName) => 
            await (
                (_entities is DefaultEntityRepository<T, TId>) 
                    ? ((DefaultEntityRepository<T, TId>)_entities).GetAndIncludeAsync(id, relationshipName, isReadonly: true)
                    : _entities.GetAndIncludeAsync(id, relationshipName)
            );

        public virtual async Task<T> CreateAsync(T entity)
        {
            return await _entities.CreateAsync(entity);
        }

        public virtual async Task<T> UpdateAsync(TId id, T entity)
        {
            var updatedEntity = await _entities.UpdateAsync(id, entity);
            return updatedEntity;
        }

        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<DocumentData> relationships)
        {
            relationshipName = _jsonApiContext.ContextGraph
                      .GetRelationshipName<T>(relationshipName);

            if (relationshipName == null)
                throw new JsonApiException(422, "Relationship name not specified.");

            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);

            if (entity == null)
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");

            var relationship = _jsonApiContext.ContextGraph
                .GetContextEntity(typeof(T))
                .Relationships
                .FirstOrDefault(r => r.InternalRelationshipName == relationshipName);

            var relationshipIds = relationships.Select(r => r.Id);

            await _entities.UpdateRelationshipsAsync(entity, relationship, relationshipIds);
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            return await _entities.DeleteAsync(id);
        }

        private IQueryable<T> ApplySortAndFilterQuery(IQueryable<T> entities)
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

        private async Task<IEnumerable<T>> ApplyPageQueryAsync(IQueryable<T> entities)
        {
            var pageManager = _jsonApiContext.PageManager;
            if (!pageManager.IsPaginated)
                return await _entities.ToListAsync(entities);

            _logger?.LogInformation($"Applying paging query. Fetching page {pageManager.CurrentPage} with {pageManager.PageSize} entities");

            return await _entities.PageAsync(entities, pageManager.PageSize, pageManager.CurrentPage);
        }

        private IQueryable<T> IncludeRelationships(IQueryable<T> entities, List<string> relationships)
        {
            _jsonApiContext.IncludedRelationships = relationships;

            foreach (var r in relationships)
                entities = _entities.Include(entities, r);

            return entities;
        }
    }
}
