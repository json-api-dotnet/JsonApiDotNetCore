using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class BaseJsonApiController<T, TId>
        : JsonApiControllerMixin
        where T : class, IIdentifiable<TId>
    {
        private readonly IResourceQueryService<T, TId> _queryService;
        private readonly IResourceCmdService<T, TId> _cmdService;
        private readonly IJsonApiContext _jsonApiContext;

        protected BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();
            _queryService = resourceService;
            _cmdService = resourceService;
        }

        protected BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceQueryService<T, TId> queryService)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();
            _queryService = queryService;
        }

        protected BaseJsonApiController(
            IJsonApiContext jsonApiContext,
            IResourceCmdService<T, TId> cmdService)
        {
            _jsonApiContext = jsonApiContext.ApplyContext<T>();
            _cmdService = cmdService;
        }

        public virtual async Task<IActionResult> GetAsync()
        {
            if (_queryService == null) throw new JsonApiException(405, "Query requests are not supported");

            var entities = await _queryService.GetAsync();

            return Ok(entities);
        }

        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            if (_queryService == null) throw new JsonApiException(405, "Query requests are not supported");

            var entity = await _queryService.GetAsync(id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            if (_queryService == null) throw new JsonApiException(405, "Query requests are not supported");

            var relationship = await _queryService.GetRelationshipsAsync(id, relationshipName);
            if (relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            if (_queryService == null) throw new JsonApiException(405, "Query requests are not supported");

            var relationship = await _queryService.GetRelationshipAsync(id, relationshipName);

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            if (_cmdService == null) throw new JsonApiException(405, "Command requests are not supported");

            if (entity == null)
                return UnprocessableEntity();

            if (!_jsonApiContext.Options.AllowClientGeneratedIds && !string.IsNullOrEmpty(entity.StringId))
                return Forbidden();

            entity = await _cmdService.CreateAsync(entity);

            return Created($"{HttpContext.Request.Path}/{entity.Id}", entity);
        }

        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            if (_cmdService == null) throw new JsonApiException(405, "Command requests are not supported");

            if (entity == null)
                return UnprocessableEntity();

            var updatedEntity = await _cmdService.UpdateAsync(id, entity);

            if (updatedEntity == null)
                return NotFound();

            return Ok(updatedEntity);
        }

        public virtual async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] List<DocumentData> relationships)
        {
            if (_cmdService == null) throw new JsonApiException(405, "Command requests are not supported");

            await _cmdService.UpdateRelationshipsAsync(id, relationshipName, relationships);

            return Ok();
        }

        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            if (_cmdService == null) throw new JsonApiException(405, "Command requests are not supported");

            var wasDeleted = await _cmdService.DeleteAsync(id);

            if (!wasDeleted)
                return NotFound();

            return NoContent();
        }
    }
}
