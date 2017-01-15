using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T> : JsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<T, int> entityRepository,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }

    public class JsonApiController<T, TId> : Controller where T : class, IIdentifiable<TId>
    {
        private readonly IEntityRepository<T, TId> _entities;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly ILogger _logger;

        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<T, TId> entityRepository,
            ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;

            _logger = loggerFactory.CreateLogger<JsonApiController<T, TId>>();
            _logger.LogTrace($@"JsonApiController activated with ContextGraph: 
                {JsonConvert.SerializeObject(jsonApiContext.ContextGraph)}");
        }

        public JsonApiController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<T, TId> entityRepository)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
        }

        [HttpGet]
        public virtual IActionResult Get()
        {
            ApplyContext();

            var entities = _entities.Get().ToList();

            return Ok(entities);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            ApplyContext();

            var entity = await _entities.GetAsync(id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpGet("{id}/{relationshipName}")]
        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            ApplyContext();

            relationshipName = _jsonApiContext.ContextGraph.GetRelationshipName<T>(relationshipName);

            if (relationshipName == null)
                return NotFound();

            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);

            if (entity == null)
                return NotFound();

            _logger?.LogInformation($"Looking up relationship '{relationshipName}' on {entity.GetType().Name}");

            var relationship = _jsonApiContext.ContextGraph
                .GetRelationship<T>(entity, relationshipName);

            if (relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            ApplyContext();

            if (entity == null)
                return BadRequest();

            await _entities.CreateAsync(entity);

            return Created(HttpContext.Request.Path, entity);
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            ApplyContext();

            if (entity == null)
                return BadRequest();

            var updatedEntity = await _entities.UpdateAsync(id, entity);

            return Ok(updatedEntity);
        }

        // [HttpPatch("{id}/{relationship}")]
        // public virtual IActionResult PatchRelationship(int id, string relation) 
        // {
        //     return Ok("Patch Id/relationship");
        // }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            ApplyContext();

            var wasDeleted = await _entities.DeleteAsync(id);

            if (!wasDeleted)
                return NotFound();

            return Ok();
        }

        // [HttpDelete("{id}/{relationship}")]
        // public virtual IActionResult Delete(int id, string relation) 
        // {
        //     return Ok("Delete Id/relationship");
        // }

        private void ApplyContext()
        {
            var routeData = HttpContext.GetRouteData();
            _jsonApiContext.RequestEntity = _jsonApiContext.ContextGraph.GetContextEntity(typeof(T));
            _jsonApiContext.ApplyContext(HttpContext);
        }
    }
}
