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
    public class JsonApiController<T> : JsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiController(
            ILoggerFactory loggerFactory,
            DbContext context, 
            IJsonApiContext jsonApiContext)
            : base(loggerFactory, context, jsonApiContext)
            { }
    }

    public class JsonApiController<T, TId> : Controller where T : class, IIdentifiable<TId>
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

            _logger = loggerFactory.CreateLogger<JsonApiController<T, TId>>();
            _logger.LogTrace($@"JsonApiController activated with ContextGraph: 
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
            var entities = _dbSet.ToList();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public virtual IActionResult Get(TId id) 
        {
            var entity = _dbSet.FirstOrDefault(e => e.Id.Equals(id));
            
            if(entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpGet("{id}/{relationshipName}")]
        public virtual IActionResult GetRelationship(TId id, string relationshipName) 
        {
            relationshipName = _jsonApiContext.ContextGraph.GetRelationshipName<T>(relationshipName);

            if(relationshipName == null)
                return NotFound();

            var entity = _dbSet
                .Include(relationshipName)
                .FirstOrDefault(e => e.Id.Equals(id));
            
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
            if(entity == null)
                return BadRequest();
            
            var oldEntity = _dbSet.FirstOrDefault(e => e.Id.Equals(id));
            if(oldEntity == null)
                return NotFound();

            var requestEntity = _jsonApiContext.RequestEntity;

            requestEntity.Attributes.ForEach(attr => {
                attr.SetValue(oldEntity, attr.GetValue(entity));
            });

            _context.SaveChanges();

            return Ok(oldEntity);
        }

        // [HttpPatch("{id}/{relationship}")]
        // public virtual IActionResult PatchRelationship(int id, string relation) 
        // {
        //     return Ok("Patch Id/relationship");
        // }

        [HttpDelete("{id}")]
        public virtual IActionResult Delete(TId id) 
        {
            var entity = _dbSet.FirstOrDefault(e => e.Id.Equals(id));
            if(entity == null)
                return NotFound();
            
            _dbSet.Remove(entity);
            _context.SaveChanges();
            
            return Ok();
        }

        // [HttpDelete("{id}/{relationship}")]
        // public virtual IActionResult Delete(int id, string relation) 
        // {
        //     return Ok("Delete Id/relationship");
        // }
    }
}
