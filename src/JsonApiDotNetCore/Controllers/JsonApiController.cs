using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T> 
    : JsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<T, int> entityRepository,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }

    public class JsonApiController<T, TId> 
    : JsonApiControllerMixin where T : class, IIdentifiable<TId>
    {
        private readonly IEntityRepository<T, TId> _entities;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly ILogger _logger;

        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<T, TId> entityRepository,
            ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();

            _entities = entityRepository;

            _logger = loggerFactory.CreateLogger<JsonApiController<T, TId>>();
            _logger.LogTrace($@"JsonApiController activated with ContextGraph: 
                {JsonConvert.SerializeObject(jsonApiContext.ContextGraph)}");
        }

        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<T, TId> entityRepository)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            var entities = _entities.Get();

            entities = ApplySortAndFilterQuery(entities);

            if (_jsonApiContext.QuerySet != null && _jsonApiContext.QuerySet.IncludedRelationships != null && _jsonApiContext.QuerySet.IncludedRelationships.Count > 0)
                entities = IncludeRelationships(entities, _jsonApiContext.QuerySet.IncludedRelationships);

            if (_jsonApiContext.Options.IncludeTotalRecordCount)
                _jsonApiContext.PageManager.TotalRecords = await entities.CountAsync();

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);

            return Ok(pagedEntities);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            T entity;
            if(_jsonApiContext.QuerySet?.IncludedRelationships != null)
                entity = await _getWithRelationshipsAsync(id);
            else
                entity = await _entities.GetAsync(id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        private async Task<T> _getWithRelationshipsAsync(TId id)
        {
            var query = _entities.Get();
            _jsonApiContext.QuerySet.IncludedRelationships.ForEach(r =>
            {
                query = _entities.Include(query, r.ToProperCase());
            });
            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        [HttpGet("{id}/relationships/{relationshipName}")]
        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            _jsonApiContext.IsRelationshipData = true;

            return await GetRelationshipAsync(id, relationshipName);
        }

        [HttpGet("{id}/{relationshipName}")]
        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            relationshipName = _jsonApiContext.ContextGraph
                .GetRelationshipName<T>(relationshipName.ToProperCase());

            if (relationshipName == null)
            {
                _logger?.LogInformation($"Relationship name not specified returning 422");
                return UnprocessableEntity();
            }

            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);

            if (entity == null)
                return NotFound();

            var relationship = _jsonApiContext.ContextGraph
                .GetRelationship<T>(entity, relationshipName);

            return Ok(relationship);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            if (entity == null)
            {
                _logger?.LogInformation($"Entity cannot be null returning 422");
                return UnprocessableEntity();
            }

            var stringId = entity.Id.ToString();
            if(stringId.Length > 0 && stringId != "0")
                return Forbidden();

            await _entities.CreateAsync(entity);

            return Created(HttpContext.Request.Path, entity);
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            if (entity == null)
            {
                _logger?.LogInformation($"Entity cannot be null returning 422");
                return UnprocessableEntity();
            }

            var updatedEntity = await _entities.UpdateAsync(id, entity);

            return Ok(updatedEntity);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            var wasDeleted = await _entities.DeleteAsync(id);

            if (!wasDeleted)
                return NotFound();

            return Ok();
        }

        private IQueryable<T> ApplySortAndFilterQuery(IQueryable<T> entities)
        {
            var query = _jsonApiContext.QuerySet;

            if(_jsonApiContext.QuerySet == null)
                return entities;

            if(query.Filters.Count > 0)
                foreach(var filter in query.Filters)
                    entities = _entities.Filter(entities, filter);

            if(query.SortParameters != null && query.SortParameters.Count > 0)
                entities = _entities.Sort(entities, query.SortParameters);

            return entities;
        }

        private async Task<IEnumerable<T>> ApplyPageQueryAsync(IQueryable<T> entities)
        {
            var pageManager = _jsonApiContext.PageManager;
            if(!pageManager.IsPaginated)
                return entities;

            var query = _jsonApiContext.QuerySet?.PageQuery ?? new PageQuery();

            _logger?.LogInformation($"Applying paging query. Fetching page {pageManager.CurrentPage} with {pageManager.PageSize} entities");

            return await _entities.PageAsync(entities, pageManager.PageSize, pageManager.CurrentPage);
        }

        private IQueryable<T> IncludeRelationships(IQueryable<T> entities, List<string> relationships)
        {
            _jsonApiContext.IncludedRelationships = relationships;

            foreach(var r in relationships)
                entities = _entities.Include(entities, r.ToProperCase());

            return entities;
        }
    }
}
