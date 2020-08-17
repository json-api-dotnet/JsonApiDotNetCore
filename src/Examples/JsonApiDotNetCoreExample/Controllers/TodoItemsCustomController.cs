using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers
{
    [ApiController]
    [DisableRoutingConvention, Route("custom/route/todoItems")]
    public class TodoItemsCustomController : CustomJsonApiController<TodoItem>
    {
        public TodoItemsCustomController(
            IJsonApiOptions options,
            IResourceService<TodoItem> resourceService) 
            : base(options, resourceService)
        { }
    }

    public class CustomJsonApiController<T>
    : CustomJsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public CustomJsonApiController(
            IJsonApiOptions options,
            IResourceService<T, int> resourceService)
            : base(options, resourceService)
        {
        }
    }

    public class CustomJsonApiController<T, TId>
    : ControllerBase where T : class, IIdentifiable<TId>
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceService<T, TId> _resourceService;

        private IActionResult Forbidden()
        {
            return new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }

        public CustomJsonApiController(
            IJsonApiOptions options,
            IResourceService<T, TId> resourceService)
        {
            _options = options;
            _resourceService = resourceService;
        }

        public CustomJsonApiController(
            IResourceService<T, TId> resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var resources = await _resourceService.GetAsync();
            return Ok(resources);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(TId id)
        {
            try
            {
                var resource = await _resourceService.GetAsync(id);
                return Ok(resource);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/relationships/{relationshipName}")]
        public async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            try
            {
                var relationship = await _resourceService.GetRelationshipAsync(id, relationshipName);
                return Ok(relationship);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/{relationshipName}")]
        public async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            var relationship = await _resourceService.GetSecondaryAsync(id, relationshipName);
            return Ok(relationship);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] T resource)
        {
            if (resource == null)
                return UnprocessableEntity();

            if (_options.AllowClientGeneratedIds && !string.IsNullOrEmpty(resource.StringId))
                return Forbidden();

            resource = await _resourceService.CreateAsync(resource);

            return Created($"{HttpContext.Request.Path}/{resource.Id}", resource);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAsync(TId id, [FromBody] T resource)
        {
            if (resource == null)
                return UnprocessableEntity();

            try
            {
                var updated = await _resourceService.UpdateAsync(id, resource);
                return Ok(updated);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] List<ResourceObject> relationships)
        {
            await _resourceService.UpdateRelationshipAsync(id, relationshipName, relationships);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(TId id)
        {
            await _resourceService.DeleteAsync(id);
            return NoContent();
        }
    }
}
