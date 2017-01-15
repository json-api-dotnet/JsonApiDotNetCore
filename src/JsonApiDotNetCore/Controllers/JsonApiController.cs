using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T> : Controller where T : class, IIdentifiable<int>
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly ILogger _logger;

        public JsonApiController(
            ILoggerFactory loggerFactory,
            DbContext context, 
            IJsonApiContext jsonApiContext)
        { 
            _context = context;
            _dbSet = context.GetDbSet<T>();
            _jsonApiContext = jsonApiContext;

            _logger = loggerFactory.CreateLogger<JsonApiController<T>>();
            _logger.LogInformation($@"JsonApiController activated with ContextGraph: 
                {JsonConvert.SerializeObject(jsonApiContext.ContextGraph)}");
        }

        public JsonApiController(
            DbContext context, 
            IJsonApiContext jsonApiContext)
        { 
            _context = context;
            _dbSet = context.GetDbSet<T>();
            _jsonApiContext = jsonApiContext;
        }

        [HttpGet]
        public virtual IActionResult Get() 
        {
            _logger.LogInformation("");
            var entities = _dbSet.ToList();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public virtual IActionResult Get(int id) 
        {
            var entity = _dbSet.FirstOrDefault(e => e.Id == id);
            
            if(entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpGet("{id}/{relationshipName}")]
        public virtual IActionResult GetRelationship(int id, string relationshipName) 
        {
            relationshipName = _jsonApiContext.ContextGraph.GetRelationshipName<T>(relationshipName);

            if(relationshipName == null)
                return NotFound();

            var entity = _dbSet
                .Include(relationshipName)
                .FirstOrDefault(e => e.Id == id);
            
            if(entity == null)
                return NotFound();

            _logger?.LogInformation($"Looking up relationship '{relationshipName}' on {entity.GetType().Name}");

            var relationship = _jsonApiContext.ContextGraph
                .GetRelationship<T>(entity, relationshipName);

            if(relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        [HttpPost]
        public virtual IActionResult Post([FromBody] T entity) 
        {
            if(entity == null)
                return BadRequest();
            
            _dbSet.Add(entity);
            _context.SaveChanges();

            return Created(HttpContext.Request.Path, entity);
        }

        [HttpPatch("{id}")]
        public virtual IActionResult Patch(int id, [FromBody] T entity) 
        {
            
            return Ok("Patch Id");
        }

        [HttpPatch("{id}/{relationship}")]
        public virtual IActionResult PatchRelationship(int id, string relation) 
        {
            return Ok("Patch Id/relationship");
        }

        [HttpDelete("{id}")]
        public virtual IActionResult Delete(int id) 
        {
            return Ok("Delete Id");
        }

        [HttpDelete("{id}/{relationship}")]
        public virtual IActionResult Delete(int id, string relation) 
        {
            return Ok("Delete Id/relationship");
        }
    }
}
