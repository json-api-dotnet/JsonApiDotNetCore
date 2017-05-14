using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T>
    : JsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }

    public class JsonApiController<T, TId>
    : JsonApiControllerMixin where T : class, IIdentifiable<TId>
    {
        private readonly ILogger _logger;
        private readonly IResourceService<T, TId> _resourceService;
        private readonly IJsonApiContext _jsonApiContext;

        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService,
            ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();
            _resourceService = resourceService;
            _logger = loggerFactory.CreateLogger<JsonApiController<T, TId>>();
        }

        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();
            _resourceService = resourceService;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAsync()
        {
            var entities = await _resourceService.GetAsync();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            var entity = await _resourceService.GetAsync(id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpGet("{id}/relationships/{relationshipName}")]
        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            var relationship = _resourceService.GetRelationshipsAsync(id, relationshipName);
            if(relationship == null) 
                return NotFound();
            
            return await GetRelationshipAsync(id, relationshipName);
        }

        [HttpGet("{id}/{relationshipName}")]
        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            var relationship = await _resourceService.GetRelationshipAsync(id, relationshipName);                
            return Ok(relationship);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            if (entity == null)
                return UnprocessableEntity();

            if (!_jsonApiContext.Options.AllowClientGeneratedIds && !string.IsNullOrEmpty(entity.StringId))
                return Forbidden();

            entity = await _resourceService.CreateAsync(entity);

            return Created($"{HttpContext.Request.Path}/{entity.Id}", entity);
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            if (entity == null)
                return UnprocessableEntity();
            
            var updatedEntity = await _resourceService.UpdateAsync(id, entity);
            
            if(updatedEntity == null)
                return NotFound();

            return Ok(updatedEntity);
        }

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public virtual async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] List<DocumentData> relationships)
        {
            await _resourceService.UpdateRelationshipsAsync(id, relationshipName, relationships);
            return Ok();
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            var wasDeleted = await _resourceService.DeleteAsync(id);

            if (!wasDeleted)
                return NotFound();

            return NoContent();
        }
    }
}
