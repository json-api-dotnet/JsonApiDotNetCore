using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableRoutingConvention, Route("custom/route/todo-items")]
    public class TodoItemsCustomController : CustomJsonApiController<TodoItem>
    {
        public TodoItemsCustomController(
            IJsonApiOptions options,
            IResourceService<TodoItem> resourceService,
            ILoggerFactory loggerFactory) 
            : base(options, resourceService, loggerFactory)
        { }
    }

    public class CustomJsonApiController<T>
    : CustomJsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public CustomJsonApiController(
            IJsonApiOptions options,
            IResourceService<T, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(options, resourceService, loggerFactory)
        {
        }
    }

    public class CustomJsonApiController<T, TId>
    : ControllerBase where T : class, IIdentifiable<TId>
    {
        private readonly ILogger _logger;
        private readonly IJsonApiOptions _options;
        private readonly IResourceService<T, TId> _resourceService;

        protected IActionResult Forbidden()
        {
            return new StatusCodeResult(403);
        }

        public CustomJsonApiController(
            IJsonApiOptions options,
            IResourceService<T, TId> resourceService,
            ILoggerFactory loggerFactory)
        {
            _options = options;
            _resourceService = resourceService;
            _logger = loggerFactory.CreateLogger<JsonApiController<T, TId>>();
        }

        public CustomJsonApiController(
            IResourceService<T, TId> resourceService)
        {
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
            var relationship = _resourceService.GetRelationshipAsync(id, relationshipName);
            if (relationship == null)
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

            if (_options.AllowClientGeneratedIds && !string.IsNullOrEmpty(entity.StringId))
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

            if (updatedEntity == null)
                return NotFound();

            return Ok(updatedEntity);
        }

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public virtual async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] List<ResourceObject> relationships)
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
